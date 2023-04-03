<?php

include "./models/car/car.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Car();

    print $user->getMarca();
} else {
    header("Location: /");
}
