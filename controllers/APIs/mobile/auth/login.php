<?php

include "./models/configuration/index.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $config = new Configuration();

    $rendMin                    = (string)$_POST["rend_min"];
    $numHourFree                = (string)$_POST["num_hour_free"];
    $quantMaxSpace              = (string)$_POST["quant_max_space"];

    DBController::$data = [
        "rendMin"       => $rendMin,
        "numHourFree"   => $numHourFree,
        "quantMaxSpace" => $quantMaxSpace,
    ];

    echo json_encode(["message" => "hello world"]);
    // print json_encode($config->updateConfiguration());
} else {
    header("Location: /");
}
