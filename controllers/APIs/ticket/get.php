<?php

include "./models/ticket/ticket.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Ticket();

    print($user->GetAllTicket());
} else {
    header("Location: /");
}
