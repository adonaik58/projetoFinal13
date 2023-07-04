<?php

include "./models/ticket/ticket.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Ticket();
    $request = (object)$_GET;

    DBController::$data = [
        "consumer_name" => $request->consumer_name,
        "date_entrace" => $request->date_entrace,
        "date_outside" => $request->date_outside,
        "bi" => $request->bi,
        "brand" => $request->brand,
        "model" => $request->model,
        "code" => $request->code,
        "order_by" => $request->order_by,
        "order" => $request->order,
        "plac" => $request->plac,
    ];
    print $user->GetAllTickesStory();
} else {
    header("Location: /");
}
