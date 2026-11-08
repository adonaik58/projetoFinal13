<?php

// namespace App\DB;

// use PDO;
// use PDOException;
require("./models/utils/statusCode.php");

class DBController extends HttpStatusCode {

   protected $connection;

   private $serverName = "localhost:3306";
   private $username = "root";
   private $password = "";

   public static array $data;

   public function __construct($var = null) {
      try {
         $conn = new PDO("mysql:host=$this->serverName;dbname=parking", $this->username, $this->password);
         // set the PDO error mode to exception
         $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
         $conn->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_OBJ);
      } catch (PDOException $e) {
         // print json_encode(array(
         //    "status" => false,
         //    "Message" => $e->getMessage()
         // ));
         // return false;
         return [];
      }
      $this->connection = $conn;
      return $conn;
   }

   /**
    * @param string|null $table
    * @param array $data
    * @param string $type
    * @return mixed
    */
   public array $responses = [];

   public function closeSession() {

      $breakApartToken = explode(".", $_COOKIE["token"]);
      $data = (string)"";
      for ($i = 0; $i < count($breakApartToken); $i++) {
         if ($i === 2) {
            $data = json_decode(base64_decode($breakApartToken[$i]));
         }
      };

      $name = $data->nome;
      $password = $data->senha;
      if (isset($_COOKIE['token'])) {
         $query = "UPDATE employee SET `active` = 0 WHERE `nome` = '{$name}' AND `senha` = '{$password}'";
         $response = $this->connection->prepare($query);
         if ($response->execute()) {
            unset($_COOKIE['token']);
            setcookie("token", "", time() - 300, "/");
         }
         header("Location: /login");
      } else {
         header("Location: /");
      }
   }

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
               $this->responses = array(
                  "status" => true,
                  "message" => "Sucesso!",
                  "token" => base64_encode(json_encode($data->id)) . "." . base64_encode(json_encode($data->code)) . "." . base64_encode(json_encode($data))
               );

               $query = "UPDATE {$table} SET `active` = 1 WHERE `nome` = '{$name}' AND `senha` = '{$password}'";
               $response = $this->connection->prepare($query);
               if ($response->execute())
                  return json_encode($this->responses);
               else {
                  $this->responses = array(
                     "status" => false,
                     "message" => "Não foi possível logar!",
                  );
                  http_response_code(+self::METHOD_NOT_ALLOWED);
                  return json_encode($this->responses);
               }
            } else {
               $this->responses = array(
                  "status" => false,
                  "message" => "Usuário não encontrado!",
               );
               http_response_code(+self::METHOD_NOT_ALLOWED);
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
         http_response_code(+self::OK);
         return json_encode($this->responses);
      } else {
         $this->responses = array(
            "status" => false,
            "message" => "Erro ao adicionar",
         );
         http_response_code(+self::EXPECTATION_FAILED);
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

   public function select(string $query, bool $fetchType = null): mixed {
      $result = null;
      if ($this->connection) {
         if (!is_null($fetchType)) {
            $result = $this->connection->query($query);
         } else {
            $result = $this->connection->prepare($query);
         }
         if ($result->execute()) {
            http_response_code(self::OK);
            return $result->fetchAll(PDO::FETCH_OBJ);
         } else {
            http_response_code(self::EXPECTATION_FAILED);
            return [];
         }
      } else {
         http_response_code(self::EXPECTATION_FAILED);
         return [];
      }
   }

   public function insert(string $table, array $columns): mixed {
      $values = self::$data;
      $placeholders = array_fill(0, count($values), '?');
      $columns = implode(',', $columns);
      $placeholders = implode(',', $placeholders);

      $query = "INSERT INTO $table ($columns) VALUES ($placeholders)";
      $stmt = $this->connection->prepare($query);
      try {
         $stmt->execute($values);
         http_response_code(self::OK);
         return ["status" => true];
      } catch (PDOException $e) {
         http_response_code(self::EXPECTATION_FAILED);
         return ["status" => false, "message" => $e->getMessage()];
      }
   }


   public function update(string $query): mixed {
      $result = $this->connection->prepare($query);
      try {
         $result->execute();
         http_response_code(self::OK);
         return ["status" => true];
      } catch (PDOException $e) {
         http_response_code(self::EXPECTATION_FAILED);
         return ["status" => false, "message" => $e->getMessage()];
      }
   }

   public function delete(string $query): mixed {
      $result = $this->connection->prepare($query);
      try {
         $result->execute();
         http_response_code(self::OK);
         return ["status" => true];
      } catch (PDOException $e) {
         http_response_code(self::EXPECTATION_FAILED);
         return ["status" => false, "message" => $e->getMessage()];
      }
   }
}
