<?php require_once '../internals/backend.php'; ?>
<?php require_once '../internals/utils.php'; ?>
<?php init("admin"); ?>
<!doctype html>

<html lang="en">
<head>
	<meta charset="utf-8">
    <link rel="stylesheet" href="pure-min.css"/>
	<link rel="stylesheet" type="text/css" href="admin.css">
</head>

<body id="rootbox">

    <script src="jquery-3.1.0.min.js"></script>

    <h1><a href="index.php">Cannon Conquest | Admin Page</a></h1>

    <?php
	function fmtLevelID($id) {
		if ($id == '{b16b00b5-0001-4000-9999-000000000002}') return "TUTORIAL";

		return (int)substr($id, 25, 6) . " - " . (int)substr($id, 31, 6);
	}
    ?>

    <?php

        $page = 0;
        if (!empty($_GET['page'])) $page = $_GET['page'];
        $entrycount = getEntryCount();
    ?>

    <div class="tablebox">
        <table class="sqltab pure-table pure-table-bordered sortable">
            <thead>
                <tr>
                    <th style='width: 170px'>Username</th>
                    <th>Level</th>
                    <th>Difficulty</th>
                    <th>Time</th>
                    <th style='width: 170px'>Last changed</th>
                </tr>
            </thead>
            <?php foreach (getAllEntries($page, 500) as $entry): ?>
                <tr>
                    <td><a href="userinfo.php?id=<?php echo $entry['userid']; ?>"><?php echo $entry['username']; ?></a> (<?php echo $entry['userid']; ?>)</td>
                    <td title="<?php echo $entry['levelid']; ?>" >
                        <a href="levelscores.php?id=<?php echo $entry['levelid']; ?>">
							<?php echo fmtLevelID($entry['levelid']); ?>
                        </a>
                    </td>
                    <td><?php echo $entry['difficulty']; ?></td>
                    <td title="<?php echo $entry['best_time']; ?>ms" ><?php echo gmdate("H:i:s", $entry['best_time']/1000.0); ?></td>
                    <td><?php echo $entry['last_changed']; ?></td>
                </tr>
            <?php endforeach; ?>
        </table>
        <div class="pagination_row">
            <?php for ($i=0; $i < ceil($entrycount/500); $i++ ): ?>
                <?php if ($i != $page): ?>
                    <a class="pagination_link" href="entrylist.php?page=<?php echo $i; ?>"><?php echo ($i+1); ?></a>
                <?php else: ?>
                    <a class="pagination_curr"><?php echo ($i+1); ?></a>
                <?php endif; ?>
            <?php endfor; ?>
        </div>
    </div>

    <?php printSQLStats(); ?>

    <script src="sorttable.js"></script>
</body>
</html>