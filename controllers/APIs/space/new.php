<?php


include "./models/space/space.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

   // $name       =     $_POST["name"];
   // $fullName   =     $_POST["fullName"];
   // $type       =     $_POST["acess"];
   // $password   =     $_POST["passwordConfirme"];



   // // echo "<pre>";
   // // print_r($_SERVER);
   // $table = "employee";

   $user = new Space();


   print_r($user->createSpace());
} else {
   echo header("location: /");
}
