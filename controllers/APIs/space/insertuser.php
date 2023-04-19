<?php
include "./models/space/space.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

    $username       = (string)$_POST["username"];
    $bi             = (string)$_POST["bi"];
    $age            = (string)$_POST["age"];
    $value          = (string)$_POST["value"] ??= 0;
    $brand          = (string)$_POST["brand"];
    $model          = (string)$_POST["model"] ?? "";
    $color          = (string)$_POST["color"];
    $plac           = (string)$_POST["plac"];
    $sID            = (string)$_POST["spaceID"];
    $dateInserted   = date("Y-m-d H:i:s");

    DBController::$data = [
        "username"      => $username,
        "bi"            => $bi,
        "age"           => $age,
        (int)"value"    => $value,
        "brand"         => $brand,
        "model"         => $model,
        "color"         => $color,
        "plac"          => $plac,
        "sID"           => $sID,
        "date"          => $dateInserted
    ];

    $result = new Space();
    print(json_encode($result->createConsumer()));
} else {
    header("Location: /");
}
