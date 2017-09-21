<?php

if(count(get_included_files()) ==1) exit("Direct access not permitted.");

require_once 'backend.php';

function getUserCount() {
	global $pdo;

	return $pdo->query('SELECT COUNT(*) FROM users WHERE score > 0')->fetch(PDO::FETCH_NUM)[0];
}

function getActiveUserCount($days) {
	global $pdo;

	return $pdo->query('SELECT COUNT(*) FROM users WHERE score > 0 AND last_online >= now() - INTERVAL ' . $days . ' DAY')->fetch(PDO::FETCH_NUM)[0];
}

function getErrorCount() {
	global $pdo;

	return $pdo->query('SELECT COUNT(*) FROM error_log')->fetch(PDO::FETCH_NUM)[0];
}

function getRemainingErrorCount($versionfilter = "") {
	if ($versionfilter == "") {
		global $pdo;

		return $pdo->query('SELECT COUNT(*) FROM error_log WHERE acknowledged = 0')->fetch(PDO::FETCH_NUM)[0];
	} else {
		global $pdo;

		$stmt = $pdo->prepare("SELECT COUNT(*) FROM error_log WHERE acknowledged = 0 AND app_version = :filter");
		$stmt->bindValue(':filter', $versionfilter, PDO::PARAM_STR);
		$stmt->execute();
		return $stmt->fetch(PDO::FETCH_NUM)[0];
	}
}

function getEntryCount() {
	global $pdo;

	return $pdo->query('SELECT COUNT(*) FROM level_highscores')->fetch(PDO::FETCH_NUM)[0];
}

function getTotalHighscore() {
	global $pdo;

	return $pdo->query('SELECT MAX(score) FROM users WHERE score > 0')->fetch(PDO::FETCH_NUM)[0];
}

function getNewErrorsOverview() {
	global $pdo;
	global $config;

	$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE acknowledged = 0 AND error_log.app_version = :vvv ORDER BY error_log.timestamp DESC LIMIT 128");
	$stmt->bindValue(':vvv', $config['latest_version'], PDO::PARAM_STR);
	$stmt->execute();
	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getRemainingErrors($versionfilter = "", $page, $pagesize = 1000) {
	global $pdo;

	if ($versionfilter != "") {
		$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE acknowledged = 0 AND error_log.app_version = :vvv LIMIT :ps OFFSET :po");
		$stmt->bindValue(':vvv', $versionfilter, PDO::PARAM_STR);
		$stmt->bindValue(':po', $page * $pagesize, PDO::PARAM_INT);
		$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
		$stmt->execute();
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	} else {
		$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE acknowledged = 0 LIMIT :ps OFFSET :po");
		$stmt->bindValue(':po', $page * $pagesize, PDO::PARAM_INT);
		$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
		$stmt->execute();
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	}
}

function countRemainingErrors($versionfilter = "") {
	global $pdo;

	if ($versionfilter != "") {
		$stmt = $pdo->prepare("SELECT COUNT(*) FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE acknowledged = 0 AND error_log.app_version = :vvv");
		$stmt->bindValue(':vvv', $versionfilter, PDO::PARAM_STR);
		$stmt->execute();
		return $stmt->fetch(PDO::FETCH_NUM)[0];
	} else {
		$stmt = $pdo->prepare("SELECT COUNT(*) FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE acknowledged = 0");
		$stmt->execute();
		return $stmt->fetch(PDO::FETCH_NUM)[0];
	}
}

function getAllErrors($versionfilter, $page, $pagesize = 1000) {
	global $pdo;

	if ($versionfilter != "") {
		$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE error_log.app_version = :vvv LIMIT :ps OFFSET :po");
		$stmt->bindValue(':po', $page * $pagesize, PDO::PARAM_INT);
		$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
		$stmt->bindValue(':vvv', $versionfilter, PDO::PARAM_STR);
		$stmt->execute();
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	} else {
		$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid LIMIT :ps OFFSET :po");
		$stmt->bindValue(':po', $page * $pagesize, PDO::PARAM_INT);
		$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
		$stmt->execute();
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	}
}

function countAllErrors($versionfilter) {
	global $pdo;

	if ($versionfilter != "") {
		$stmt = $pdo->prepare("SELECT COUNT(*) FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE error_log.app_version = :vvv");
		$stmt->bindValue(':vvv', $versionfilter, PDO::PARAM_STR);
		$stmt->execute();
		return $stmt->fetch(PDO::FETCH_NUM)[0];
	} else {
		$stmt = $pdo->prepare("SELECT COUNT(*)  FROM error_log LEFT JOIN users ON error_log.userid = users.userid");
		$stmt->execute();
		return $stmt->fetch(PDO::FETCH_NUM)[0];
	}
}

function getErrorOverviewByID() {
	global $pdo;

	return $pdo->query('SELECT error_log.exception_id AS exception_id, COUNT(*) AS count_all, (SELECT COUNT(*) FROM error_log AS e2 WHERE e2.exception_id = error_log.exception_id AND e2.acknowledged=0 ) AS count_noack FROM error_log GROUP BY error_log.exception_id')->fetchAll(PDO::FETCH_ASSOC);
}

function getNewErrorOverviewByID() {
	global $pdo;

	return $pdo->query('SELECT error_log.exception_id AS exception_id, (SELECT COUNT(*) FROM error_log AS e2 WHERE e2.exception_id = error_log.exception_id) AS count_all, COUNT(*) AS count_noack FROM error_log WHERE error_log.acknowledged=0 GROUP BY error_log.exception_id')->fetchAll(PDO::FETCH_ASSOC);
}

function getUserErrors($uid) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT *, error_log.app_version AS app_version, users.app_version AS user_app_version FROM error_log LEFT JOIN users ON error_log.userid = users.userid WHERE users.userid = :uid AND score > 0");
	$stmt->bindValue(':uid', $uid, PDO::PARAM_INT);
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getUsers($all, $reg, $days, $page, $pagesize = 1000) {
	global $pdo;

	$cond = "WHERE 1=1";
	if (!$all)      $cond = $cond . " AND score > 0";
	if ($reg)       $cond = $cond . " AND is_auto_generated = 0";
	if ($days >= 0) $cond = $cond . " AND last_online >= now() - INTERVAL $days DAY";

	$stmt = $pdo->prepare("SELECT * FROM users $cond LIMIT :ps OFFSET :po");
	$stmt->bindValue(':po', $page * $pagesize, PDO::PARAM_INT);
	$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
	$stmt->execute();
	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function countUsers($all, $reg, $days) {
	global $pdo;

	$cond = "WHERE 1=1";
	if (!$all)      $cond = $cond . " AND score > 0";
	if ($reg)       $cond = $cond . " AND is_auto_generated = 0";
	if ($days >= 0) $cond = $cond . " AND last_online >= now() - INTERVAL $days DAY";

	$stmt = $pdo->prepare('SELECT COUNT(*) FROM users ' . $cond);
	$stmt->execute();
	return $stmt->fetch(PDO::FETCH_NUM)[0];
}

function getLevelHighscores() {
	global $pdo;

	return $pdo->query('SELECT * FROM cache_levels LEFT JOIN users ON best_userid = users.userid AND users.score > 0')->fetchAll(PDO::FETCH_ASSOC);
}

function getGlobalHighscores() {
	global $pdo;

	return $pdo->query(loadSQL("get-ranking_global_top100"))->fetchAll(PDO::FETCH_ASSOC);
}

function getMultiplayerHighscores() {
	global $pdo;

	return $pdo->query("SELECT * FROM users WHERE is_auto_generated=0 AND score > 0 ORDER BY mpscore DESC LIMIT 100")->fetchAll(PDO::FETCH_ASSOC);
}

function getAllEntries($page, $pagesize) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT * FROM level_highscores LEFT JOIN users ON level_highscores.userid = users.userid WHERE score > 0 LIMIT :ps OFFSET :po");
	$stmt->bindValue(':po', $page*$pagesize, PDO::PARAM_INT);
	$stmt->bindValue(':ps', $pagesize, PDO::PARAM_INT);
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getLevelEntries($lvl) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT * FROM level_highscores LEFT JOIN users ON level_highscores.userid = users.userid WHERE levelid= :id AND score > 0");
	$stmt->bindValue(':id', $lvl, PDO::PARAM_STR);
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getLevelDiffEntries($lvl, $diff, $limit) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT * FROM level_highscores LEFT JOIN users ON level_highscores.userid = users.userid WHERE levelid= :id AND difficulty = :diff AND score > 0 ORDER BY level_highscores.best_time ASC LIMIT :lim");
	$stmt->bindValue(':id', $lvl, PDO::PARAM_STR);
	$stmt->bindValue(':diff', $diff, PDO::PARAM_STR);
	$stmt->bindValue(':lim', $limit, PDO::PARAM_INT);
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getUserEntries($uid) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT * FROM level_highscores LEFT JOIN users ON level_highscores.userid = users.userid WHERE level_highscores.userid= :uid AND score > 0");
	$stmt->bindValue(':uid', $uid, PDO::PARAM_INT);
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getWorldHighscores($worldid) {
	global $pdo;

	$stmt = $pdo->prepare(loadReplSQL('get-ranking_local_top100', '#$$FIELD$$', worldGuidToSQLField($worldid)));
	$stmt->execute();
	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function getLastCronTime() {
	global $config;

	$path = str_replace('{action}', 'cron', $config['logfile-cron']);

	if (!file_exists($path)) return "FNF";

	return date('Y-m-d H:i:s', filemtime($path));
}

function listLogFiles() {
	global $config;

	$list = [];

	$path = dirname($config['logfile-normal']);
	$dir = opendir($path);
	while($file = readdir($dir)){
		if ($file == '.' or $file == '..') continue;

		$filepath = $path . '/' . $file;

		$list[] =
			[
				'path' => $filepath,
				'name' => basename($filepath),
				'size' => filesize($filepath),
				'changedate' => date('Y-m-d H:i:s', filemtime($filepath)),
				'content' => file_get_contents($filepath),
			];
	}
	closedir($dir);

	return $list;
}

function worldGuidToSQLField($worldid)
{
	global $config;

	if ($worldid == $config['worldid_1'] ) return "w1";
	if ($worldid == $config['worldid_2'] ) return "w2";
	if ($worldid == $config['worldid_3'] ) return "w3";
	if ($worldid == $config['worldid_4'] ) return "w4";

	throw new Exception("Unknown WorldID: " . $worldid);
}

function getScoreDistribution() {
	global $pdo;

	$stmt = $pdo->prepare("SELECT score AS score, COUNT(*) AS count FROM users WHERE score > 0 GROUP BY score");
	$stmt->execute();

	return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

function countUsersByUnlock($u) {
	global $pdo;

	$stmt = $pdo->prepare("SELECT COUNT(*) FROM users WHERE unlocked_worlds LIKE CONCAT(\"%\", :u, \"%\")");
	$stmt->bindValue(':u', $u, PDO::PARAM_STR);
	$stmt->execute();

	return $stmt->fetch(PDO::FETCH_NUM)[0];
}

function countFirstPlaceUsers() {
	global $pdo;

	$stmt = $pdo->prepare("SELECT COUNT(*) FROM users GROUP BY score ORDER BY score DESC LIMIT 1");
	$stmt->execute();

	return $stmt->fetch(PDO::FETCH_NUM)[0];
}

function countZeroScoreUsers() {
	global $pdo;

	$stmt = $pdo->prepare("SELECT COUNT(*) FROM users WHERE score = 0");
	$stmt->execute();

	return $stmt->fetch(PDO::FETCH_NUM)[0];
}