<?php

include "./models/User/User.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {
   $user = new User();

   if (!isset($_GET["id"])) {
      print($user->getData());
   } else {
      $ID = (int)$_GET["id"] ?? null;
      print($user->getDataById((int)$ID));
   }
} else {
   header("Location: /");
}
