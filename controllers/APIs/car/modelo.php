<?php

include "./models/car/car.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Car();
    $marcaId =  (int)$_GET["marca_id"];
    print $user->getModelo(+$marcaId);
} else {
    header("Location: /");
}
