<?php
include "./models/ticket/ticket.php";
if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

    $id = (string)$_GET["id"];

    $spaceID = explode(".", $id)[0];
    $bi = explode(".", $id)[1];

    DBController::$data = [
        "id" => $spaceID,
        "bi" => $bi,
    ];


    $result = new Ticket();


    print json_encode($result->Fechar());
} else {
    header("Location: /");
}
