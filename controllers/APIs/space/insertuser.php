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
    if (strlen($bi) == 14) {
        if (preg_match('/^[0-9]{9}[A-Z]{2}[0-9]{3}$/', $bi)) {

            print(json_encode($result->createConsumer()));
        } else {

            print(json_encode(
                ["status" => false, "message" => "Bilhete de Identidade inválido!"]
            )
            );
        }
    } else {
        print(json_encode(
            ["status" => false, "message" => "O Bilhete de identidade contém 14 dígitos, por favor verifique!"]
        )
        );
    }
} else {
    header("Location: /");
}
