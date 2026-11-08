<?php

// use App\Employees\Authentication;

include("./models/employees/auth.php");

$name = $_POST["name"];
$password = $_POST["password"];

$table = "employee";


$employee = new Authentication();

print($employee->login($table, [
   "name" => $name,
   "password" => $password,
]));
