<?php

include "./models/space/space.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Space();
    $request = (object)$_GET;
    // print "<pre>";
    // print_r($request);

    DBController::$data = [
        "plac" => $request->plac,
        "code" => $request->code,
        "space_status" => $request->space_status,
        "order" => $request->order,
        "request" => $request,
    ];
    print $user->getSpace();
} else {
    header("Location: /");
}
