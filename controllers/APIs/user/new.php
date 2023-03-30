<?php

// use App\Employees\Authentication;

include "./models/User/User.php";

if (!isset($_SERVER["HTTP_SEC_FETCH_USER"])) {

   $name       =     $_POST["name"];
   $fullName   =     $_POST["fullName"];
   $type       =     $_POST["acess"];
   $password   =     $_POST["passwordConfirme"];



   // echo "<pre>";
   // print_r($_SERVER);
   $table = "employee";

   $user = new User();


   print $user->insertUser(
      array(
         (string)"acess"      => $type,
         (string)"name"       => $name,
         (string)"fullName"   => $fullName,
         (string)"password"   => $password,
      )
   );
} else {
   header("location: /");
}
