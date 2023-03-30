<?php

include "./models/DB/DBcontroller.php";

class User extends DBController {

   public function getData(): string {

      $query = "SELECT * FROM employee";
      $response = $this->connection->prepare($query);
      $response->execute();

      $this->responses = $response->fetchAll(PDO::FETCH_OBJ);
      return json_encode($this->responses);
   }
   public function getDataById(int $ID) {
      if (!$ID) return json_encode([
         "status"    => false,
         "message"   => "Usuário não encontrado"
      ]);
      else {
         $query = "SELECT * FROM employee WHERE `id` = {$ID}";
         $response = $this->connection->prepare($query);
         $response->execute();

         $this->responses = $response->fetchAll(PDO::FETCH_OBJ);
         return json_encode($this->responses);
      }
   }

   private function exist_With_WHERE($where) {

      $query = "SELECT * FROM employee WHERE " . $where;
      $response = $this->connection->prepare($query);
      $response->execute();
      if ($response->rowCount() > 0) {
         return true;
      } else {
         return false;
      }
   }

   public function insertUser(array $data): string {

      $type       = $data["acess"];
      $name       = $data["name"];
      $full_name  = $data["fullName"];
      $password   = md5($data["password"]);

      if ($this->connection) {
         if (empty($name) || empty($full_name) || empty($type) || empty($password)) {
            return json_encode(
               array(
                  "status"    => false,
                  "message"   => "Algum campo está vazio"
               )
            );
         } else {
            if ($this->exist_With_WHERE("`nome` = '{$data["name"]}'")) {
               return json_encode(
                  array(
                     "status"    => false,
                     "message"   => "Existe um utilizador com o nome " . $name . ""
                  )
               );
            } else {
               $query = "INSERT INTO employee VALUES(default, '{$type}', '{$name}', '{$full_name}', '{$password}', FALSE)";
               return $this->insertData($query);
            }
         }
      } else {
         return json_encode(
            array(
               "status"    => false,
               "message"   => "Algum problema ao contactar o servidor"
            )
         );
      }
   }
   public function updateUser(array $data): string {

      $type                = $data["acess"];
      $name                = $data["name"];
      $full_name           = $data["fullName"];
      $newPassword         = md5($data["newPassword"]);
      $passwordConfirme    = md5($data["passwordConfirme"]);
      $ID                  = (int)$data["id"];

      if ($this->connection) {
         if (empty($name) || empty($full_name) || empty($type) || empty($passwordConfirme) || empty($newPassword)) {
            return json_encode(
               array(
                  "status"    => false,
                  "message"   => "algum campo está vazio"
               )
            );
         } else {

            if ($this->exist_With_WHERE("`id` = '{$ID}' AND `senha` = '{$passwordConfirme}'")) {
               $query = "UPDATE employee SET 
               `code` = '{$type}',
               `nome` = '{$name}',
               `nome_completo` = '{$full_name}',
               `senha` = '{$newPassword}' WHERE `id` = {$ID}";

               if ($this->updateData($query)) {
                  $this->responses = array(
                     "status" => true,
                     "message" => "Utilizador Atualizado",
                  );
                  return json_encode($this->responses);
               } else {
                  $this->responses = array(
                     "status" => false,
                     "message" => "Algo não funcionou!",
                  );
                  return json_encode($this->responses);
               }
            } else
               return json_encode(
                  array(
                     "status"    => false,
                     "message"   => "Senha errada!"
                  )
               );
         }
      } else {
         return json_encode(
            array(
               "status"    => false,
               "message"   => "Algum problema ao contactar o servidor"
            )
         );
      }
   }
}
