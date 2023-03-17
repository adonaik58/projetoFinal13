<?php

// use App\Employees\Authentication;

include "./models/User/User.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

   $name       =     $_POST["name"];
   $fullName   =     $_POST["fullName"];
   $type       =     $_POST["acess"];
   $passwordConfirme       =     $_POST["passwordConfirme"];
   $new_password   =     $_POST["new_password"];
   $id         =     $_GET["id"];


   // echo "<pre>";
   // print_r($_SERVER);
   $table = "employee";

   $user = new User();


   print $user->updateUser(
      array(
         (string)"id"         => $id,
         (string)"acess"      => $type,
         (string)"name"       => $name,
         (string)"fullName"   => $fullName,
         (string)"newPassword"            => $new_password,
         (string)"passwordConfirme"   => $passwordConfirme,
      )
   );
} else {
   echo header("location: /");
}
