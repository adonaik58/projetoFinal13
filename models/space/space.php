<?php

include "./models/DB/DBcontroller.php";

class Space extends DBController {

   public function insertCartInSpace() {
   }
   public function getSpace() {
      $result = $this->select("SELECT 
         idade,
         consumidores.nome as c_nome,
         consumidores.bi as bi,
         consumidores.matricula_carro as matricula,
         consumidores.cor_carro as cor,
         consumidores.data_hora_entrada,
         espacos.id as id ,
         espacos.nome as nome,
         espacos.codigo,
         espacos.ativo,
         espacos.estado,
         marcas_carros.nome as marca,
         modelos_carros.nome as modelo
         FROM espacos 
         LEFT JOIN consumidores ON consumidores.bi = espacos.bi_atribuicao
         LEFT JOIN marcas_carros ON marcas_carros.id = consumidores.id_marca_carro
         LEFT JOIN modelos_carros ON modelos_carros.id = consumidores.id_modelo_carro
         ORDER BY espacos.nome ASC
      ");
      try {
         $newObject = array();

         foreach ($result as $row) {

            // Calculando o intervalo de tempo
            $data_passada = new DateTime($row->data_hora_entrada ?? "");
            $data_atual   = new DateTime();
            $intervalo    = $data_atual->diff($data_passada);


            $year    = $intervalo->format('%y');
            $month   = $intervalo->format('%m');
            $day     = $intervalo->format('%d');
            $hour    = $intervalo->format('%h');
            $min     = $intervalo->format('%i');
            $seg     = $intervalo->format('%s');

            // Calculando idade
            $data_nascimento = new DateTime($row->idade ?? "");
            $idade = $data_atual->diff($data_nascimento);

            $ano     = $idade->format('%y');
            $mes     = $idade->format('%m');


            // calculando o preço
            $preco = (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;

            $newObject[] = [
               "id"     => $row->id,
               "c_nome" => $row->c_nome,
               "nome"   => $row->nome,
               "codigo" => $row->codigo,
               "ativo"  => $row->ativo,
               "estado" => $row->estado,
               "matricula" => $row->matricula,
               "marca" => $row->marca,
               "modelo" => $row->modelo,
               "cor" => $row->cor,
               "bi" => $row->bi,
               "preco"  => $preco,
               "idade"  => [
                  "ano" => $ano,
                  "mes" => $mes,
               ],
               "tempo_ocupado" => [
                  "ano"        => (int)$year,
                  "mes"        => (int)$month,
                  "dia"        => (int)$day,
                  "hora"       => (int)$hour,
                  "minuto"     => (int)$min,
                  "segundo"    => (int)$seg,
               ]
            ];
         }


         http_response_code(self::OK);
         return json_encode($newObject);
      } catch (\Exception) {
         http_response_code(self::EXPECTATION_FAILED);
         return json_encode(["status" => false, "message" => "Algo deu errado"]);
      }
   }
   public function getMarca() {
      $result = $this->select("SELECT * FROM marca_carros");
      if ($result) {
         http_response_code(self::OK);
         return json_encode($result);
      } else {
         http_response_code(self::EXPECTATION_FAILED);
         return json_encode(["status" => false, "message" => "Erro ao pegar carro"]);
      }
   }
   public function createSpace() {
      /* var allSpace = [];
      const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
      for (var i = 0; i < alphabet.length; i++){
         var t = 1;
         for(; t <= 10; t++){
            allSpace.push(alphabet[i]+t)
         }
      } */

      // $allSpace = [];
      // $alphabet = explode(".", "A.B.C.D.E.F.G.H.I.J.K.L.M.N.O.P.Q.R.S.T.U.V.W.X.Y.Z");

      // for ($i = 0; $i < count($alphabet); $i++) {
      //    $t = 1;
      //    for (; $t <= 10; $t++) {
      //       array_push($allSpace, $alphabet[$i] . $t);
      //    }
      // }
      // foreach ($allSpace as $each) {
      //    $uID = uniqid();
      //    $this->insertData("INSERT INTO espacos VALUES(default, '{$each}', '{$uID}', 2, 2)");
      // }
      // echo "<pre>";
      // return ($allSpace);
   }
   public function createConsumer() {
      $data = (object)(self::$data);

      $data->value ??= 0;

      // return $data;
      // ()
      $record = $this->select("SELECT * FROM espacos WHERE bi_atribuicao = '$data->bi' LIMIT 1");
      if (count($record) <= 0) {
         // $get = (object)$this->query("SELECT * FROM consumidores WHERE `bi` = '$data->bi'", 1);
         $isUpdate = (object)$this->update("UPDATE espacos SET `bi_atribuicao` ='$data->bi', `ativo` ='s', `estado` ='i' WHERE `id` ={$data->sID}");
         if ($isUpdate->status) {
            $table = 'consumidores';
            $columns = [
               'nome',
               'bi',
               'idade',
               'valor',
               'id_marca_carro',
               'id_modelo_carro',
               'cor_carro',
               'matricula_carro',
               'data_hora_entrada',
               "reservado"
            ];
            self::$data = [
               $data->username,
               $data->bi,
               $data->age,
               $data->value,
               $data->brand,
               $data->model,
               $data->color,
               $data->plac,
               $data->date,
               FALSE,
            ];

            $result = (object)$this->insert($table, $columns);
            if ($result->status) {
               $space = $this->getSpace();
               http_response_code(self::OK);
               return ["status" => true, "message" => "Consumidor inserido", "data" => json_decode($space)];
            }
            http_response_code(self::EXPECTATION_FAILED);
            return ["status" => false, "message" => "Consumidor não inserido"];
         }
         http_response_code(self::EXPECTATION_FAILED);
         return ["status" => false, "message" => "Erro ao atribuir espaço ao consumidor"];
      }
      http_response_code(self::EXPECTATION_FAILED);
      return ["status" => false, "message" => "Consumidor ainda está no espaço"];
   }
}
