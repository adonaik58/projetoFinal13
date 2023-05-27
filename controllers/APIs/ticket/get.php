<?php

include "./models/ticket/ticket.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Ticket();
    $filter = $_GET["filter"];
    $order = $_GET["order"];

    print($user->GetAllTicket($filter, $order));
} else {
    header("Location: /");
}
