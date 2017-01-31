<?php

require_once 'internals/backend.php';


function run() {
	global $pdo;

	$userid        = getParamIntOrError('userid');
	$password_old  = getParamStrOrError('password_old');
	$password_new  = getParamStrOrError('password_new');
	$username_new  = getParamStrOrError('username_new');

	$signature     = getParamStrOrError('msgk');

	check_commit_signature($signature, [$userid, $password_old, $password_new, $username_new]);

	$username_new = trim($username_new);

	//----------

	$stmt = $pdo->prepare("SELECT COUNT(*) FROM users WHERE username=:usr");
	$stmt->bindValue(':usr', $username_new, PDO::PARAM_STR);
	$stmt->execute();

	if ($stmt->fetchColumn() > 0) outputError(ERRORS::UPGRADE_USER_DUPLICATE_USERNAME, "username $username_new already exists", LOGLEVEL::DEBUG);
	if ($username_new == 'anonymous') outputError(ERRORS::UPGRADE_USER_DUPLICATE_USERNAME, "username $username_new already exists", LOGLEVEL::DEBUG);

	//----------

	$stmt = $pdo->prepare("SELECT username, password_hash, is_auto_generated FROM users WHERE userid=:id");
	$stmt->bindValue(':id', $userid, PDO::PARAM_INT);
	$stmt->execute();
	$row = $stmt->fetch(PDO::FETCH_ASSOC);

	if ($row === FALSE) outputError(ERRORS::UPGRADE_USER_INVALID_USERID, "No user with id $userid found", LOGLEVEL::DEBUG);

	if (! password_verify($password_old, $row['password_hash'])) outputError(ERRORS::UPGRADE_USER_WRONG_PASSWORD, "Wrong password supplied", LOGLEVEL::DEBUG);

	if (! $stmt['is_auto_generated']) outputError(ERRORS::UPGRADE_USER_ACCOUNT_ALREADY_SET, "The account is already ", LOGLEVEL::DEBUG);

	//----------

	$hash = password_hash($password_new, PASSWORD_BCRYPT);
	if (!$hash) throw new Exception('password_hash failure');

	$stmt = $pdo->prepare("UPDATE users SET username=:usr, password_hash=:pw, is_auto_generated=0, last_online=CURRENT_TIMESTAMP() WHERE userid=:id");
	$stmt->bindValue(':usr', $username_new, PDO::PARAM_STR);
	$stmt->bindValue(':pw', $hash, PDO::PARAM_STR);
	$stmt->bindValue(':id', $userid, PDO::PARAM_INT);
	$succ = $stmt->execute();
	if (!$succ) throw new Exception('SQL for insert user failed');

	//----------

	outputResultSuccess([]);
	logMessage("user upgraded to account ($userid -> $username_new)");
}



try {
	run();
} catch (Exception $e) {
	logError("InternalError: " . $e->getMessage() . "\n" . $e);
	outputError(Errors::INTERNAL_EXCEPTION, $e->getMessage());
}