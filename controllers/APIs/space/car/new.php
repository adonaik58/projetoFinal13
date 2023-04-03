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

// SELECT 
// 	*,modelos.id as id_modelo
//     FROM marcas_carros as marcas 
//     LEFT JOIN modelos_carros as modelos ON modelos.id_marca = marcas.id 
//     order by marcas.nome, modelos.nome asc LIMIT 10000

// SELECT 
// 	*,modelos.id as id_modelo
//     FROM marcas_carros as marcas 
//     LEFT JOIN modelos_carros as modelos ON modelos.id_marca = marcas.id 			WHERE  modelos.id is NULL 
//     order by marcas.nome, modelos.nome asc LIMIT 10000