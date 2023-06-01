<?php

include "./models/configuration/index.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $config = new Configuration();

    print json_encode($config->autoComplete());
} else {
    header("Location: /");
}
