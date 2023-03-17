<?php

// namespace App\DB;

// use PDO;
// use PDOException;

class DBController {

   protected $connection;

   private $serverName = "localhost";
   private $username = "root";
   private $password = "";

   public function __construct($var = null) {
      try {
         $conn = new PDO("mysql:host=$this->serverName;dbname=parking", $this->username, $this->password);
         // set the PDO error mode to exception
         $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
         $conn->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_OBJ);
      } catch (PDOException $e) {
         print json_encode(array(
            "status" => false,
            "Message" => $e->getMessage()
         ));
         die();
      }
      $this->connection = $conn;
   }

   /**
    * @param string|null $table
    * @param array $data
    * @param string $type
    * @return mixed
    */
   public array $responses = [];
   public function verifyExistence(string $table = null, array $data, string $type): mixed {

      $query = "";
      switch ($type) {
         case 'login':
            $name = $data["name"];
            $password = md5($data["password"]);

            $query = "SELECT * FROM {$table} WHERE `nome` = '{$name}' AND `senha` = '{$password}'";
            $response = $this->connection->prepare($query);
            $response->execute();
            if ($response->rowCount()) {
               $data = $response->fetchAll(PDO::FETCH_OBJ)[0];
               $json_convertion = json_encode(
                  [
                     "id" => $data->id,
                     "code" => $data->code,
                     "nome" => $data->nome,
                     "senha" => $data->senha
                  ]
               );
               $this->responses = array(
                  "status" => true,
                  "message" => "Sucesso!",
                  "token" => base64_encode(json_encode($data->id)) . "." . base64_encode(json_encode($data->code)) . "." . base64_encode(json_encode($data))
               );

               return json_encode($this->responses);
            } else {
               $this->responses = array(
                  "status" => false,
                  "message" => "Erro ao logar!",
               );
               return json_encode($this->responses);
            }
            break;

         case "isAuth":

            $table;
            $id = $data["id"];
            $isAdmin = $data["isAdmin"];

            $query = "SELECT * FROM {$table} WHERE `id` = '{$id}' AND `code` = '{$isAdmin}'";
            $process = $this->connection->prepare($query);
            $process->execute();

            if (count($process->fetchAll()))
               return true;
            else
               return false;


         default:
            # code...
            break;
      }
   }

   public function insertData(string $query) {

      $result = $this->connection->prepare($query);

      if ($result->execute()) {
         $this->responses = array(
            "status" => true,
            "message" => "Utilizador adicionado",
         );
         return json_encode($this->responses);
      } else {
         $this->responses = array(
            "status" => false,
            "message" => "Erro ao adicionar",
         );
         return json_encode($this->responses);
      }
   }
   public function updateData(string $query) {
      $result = $this->connection->prepare($query);

      if ($result->execute()) {

         return true;
      } else {
         $this->responses = array(
            "status" => false,
            "message" => "Erro ao adicionar",
         );
         return false;
      }
   }
}
