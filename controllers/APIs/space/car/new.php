<?php


include "./models/space/space.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

   $name       =     $_POST["name"];
   $fullName   =     $_POST["fullName"];
   $type       =     $_POST["acess"];
   $password   =     $_POST["passwordConfirme"];



   // echo "<pre>";
   // print_r($_SERVER);
   $table = "employee";

   $user = new Space();


   print $user->insertCartInSpace(
      array(
         (string)"acess"      => $type,
         (string)"name"       => $name,
         (string)"fullName"   => $fullName,
         (string)"password"   => $password,
      )
   );
} else {
   echo header("location: /");
}
