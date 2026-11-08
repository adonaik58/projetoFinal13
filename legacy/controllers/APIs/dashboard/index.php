<?php

include "./models/dashboard/index.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Dashboard();

    print json_encode($user->getDashboardResults());
} else {
    header("Location: /");
}
