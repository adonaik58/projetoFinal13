<?php

include "./models/DB/DBcontroller.php";

class Car extends DBController {

   public function getMarca() {
      $result = $this->select("SELECT * FROM marcas ORDER BY nome asc");
      if ($result) {
         http_response_code(self::OK);
         return json_encode($result);
      } else {
         http_response_code(self::EXPECTATION_FAILED);
         return json_encode([]);
      }
   }
   public function getModelo(mixed $id) {
      $result = $this->select("SELECT * FROM modelos WHERE id_marca = {$id} ORDER BY nome ASC");
      if ($result) {
         http_response_code(self::OK);
         return json_encode($result);
      } else {
         http_response_code(self::EXPECTATION_FAILED);
         return json_encode(["result" => false, "message" => "Erro ao pegar Modelo"]);
      }
   }
}
