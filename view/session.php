<?php

include "./models/dashboard/index.php";

$db = new DBController();

$db->closeSession();
// unset($_COOKIE['token']);
// setcookie("token", "", time() - 300, "/");
// if (!isset($_COOKIE['token'])) {
//     header("Location: /login");
// } else {
//     header("Location: /");
// }
