<?php

include "./models/space/space.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
    $user = new Space();

    print $user->getSpace();
} else {
    header("Location: /");
}
