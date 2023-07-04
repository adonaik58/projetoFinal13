<?php

// use App\Employees\Authentication;

include "./models/User/User.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

   $id = $_GET["id"];

   $table = "employee";
   $user = new User();

   print $user->deleteUser($id);
} else {
   echo header("location: /");
}
