<?php

// namespace App\Employees;
// use App\DB\DBController;
include("./models/DB/DBcontroller.php");

class Authentication {

   public function login($table, array $data) {
      $conn = new DBController();

      echo $conn->verifyExistence($table, $data, "login");
   }
}
