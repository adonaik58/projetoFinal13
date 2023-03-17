<?php

// use App\DB\DBController;

include "./models/DB/DBcontroller.php";
$data = (object)"";
if (!isset($_COOKIE["token"])) header("Location: /login");
else {

   $breakApartToken = explode(".", $_COOKIE["token"]);
   $data = (string)"";
   $id = (string)"";
   $adminCheck = (string)"";
   for ($i = 0; $i < count($breakApartToken); $i++) {
      if ($i === 0) $id =  json_decode(base64_decode($breakApartToken[$i]));

      if ($i === 1) $adminCheck =  json_decode(base64_decode($breakApartToken[$i]));

      if ($i === 2) {
         $data = json_decode(base64_decode($breakApartToken[$i]));
         $data->id === $id ?? header("Location: /login");
         $data->code === $adminCheck ?? header("Location: /login");
      }
   };

   $DB = new DBController();
   $OK = $DB->verifyExistence(
      "employee",
      [
         "id" => $id,
         "isAdmin" => $adminCheck
      ],
      "isAuth"
   );

   if (!$OK) header("Location: /login");


   // echo "<pre>";
   // print_r($_SERVER);
   // if ($_SERVER["REQUEST_URI"] !== "/dashboard" || $_SERVER["REQUEST_URI"] !== "/")
   //    if ($_SERVER["REQUEST_URI"] !== "/tickets")
   //       if ((int)$data->code !== 2) header("Location: /tickets");
};
