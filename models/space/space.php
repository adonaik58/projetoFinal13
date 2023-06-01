<?php

include "./models/DB/DBcontroller.php";

class Space extends DBController {

   public function insertCartInSpace() {
   }
   public function getSpace() {

      if (is_null($this->connection)) {
         http_response_code(self::EXPECTATION_FAILED);
         return json_encode(["status" => false, "message" => "Não há conexão"]);
      }
      $getLimit = $this->select("SELECT 
            quant_max_espaco AS quant,
            renda_min AS renda,
            num_hora_gratis AS num
      FROM config");


      // $sp = (object)$this->select("SELECT TIMESTAMPDIFF(HOUR, " . (string)$row->data_entrada . ", " . (string)$dateTime . ") AS hour;");
      $dateTime = date("Y-m-d H:i:s");
      $result = $this->select("SELECT 
      idade,
      c.nome as c_nome,
      c.bi as bi,
      c.matricula_carro as matricula,
      c.cor_carro as cor,
      c.data_hora_entrada as data_entrada,
      LAST_DAY(CAST(c.data_hora_entrada AS Date)) AS lastDay,
      TIMESTAMPDIFF(HOUR, c.data_hora_entrada , '$dateTime') AS hour,
      e.id as id,
      e.nome as nome,
      e.codigo,
      e.ativo,
      e.estado,
      ma.nome as marca,
      mo.nome as modelo
      FROM espacos e
      LEFT JOIN consumidores c ON c.bi = e.bi
      LEFT JOIN marcas ma ON ma.id = c.id_marca_carro
      LEFT JOIN modelos mo ON mo.id = c.id_modelo_carro
      WHERE
      -- c.data_hora_entrada is NOT NULL AND
      c.data_hora_saida is NULL
      ORDER BY e.nome ASC
      LIMIT " . $getLimit[0]->quant . ";");
      try {
         $newObject = array();

         foreach ($result as $row) {

            // Calculando o intervalo de tempo
            $data_passada = new DateTime($row->data_entrada ?? "");
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

            $preco = 0;
            if (!empty($row->data_entrada)) {

               // echo $dateTime . "e <br/>";
               // die($row->data_entrada);
               // calculando o preço
               $lastDay = explode("-", $row->lastDay)[2];
               if ((int)$row->hour >= (int)$getLimit[0]->num) {
                  $preco = (((((((($year * 12) + $month) * (int)$lastDay) + $day) * 24) - (int)$getLimit[0]->num + $hour) * 60) + $min) * (int)$getLimit[0]->renda;
               }
            }

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
      $record = $this->select("SELECT * FROM espacos WHERE `bi` = '$data->bi' LIMIT 1");
      if (count($record) <= 0) {
         // $get = (object)$this->query("SELECT * FROM consumidores WHERE `bi` = '$data->bi'", 1);
         $breakApartToken = explode(".", $_COOKIE["token"]);
         $user = (string)"";
         for ($i = 0; $i < count($breakApartToken); $i++) {
            if ($i === 2) {
               $user = json_decode(base64_decode($breakApartToken[$i]));
            }
         };
         $table = 'consumidores';
         (array)$columns = [
            'nome',
            'bi',
            'idade',
            'valor',
            'id_marca_carro',
            'id_modelo_carro',
            'cor_carro',
            'matricula_carro',
            'data_hora_entrada',
            "operador",
            "reservado"
         ];
         (array)self::$data = [
            $data->username,
            $data->bi,
            $data->age,
            $data->value,
            $data->brand,
            $data->model,
            $data->color,
            $data->plac,
            $data->date,
            $user->id,
            0
         ];

         (array)$filterData = array(
            $data->username,
            $data->age,
            $data->brand,
            $data->model,
            $data->color,
            $data->plac,
            $data->date,
         );

         foreach ($filterData as $data_) {
            if (!empty($data_)) {
               continue;
            } else return ["status" => false, "message" => "Algum precisa ser preenchido"];
         }

         $result = (object)$this->insert($table, $columns);

         if ($result->status) {
            $isUpdate = (object)$this->update("UPDATE espacos
             SET `bi`   = '$data->bi',
            `ativo`     = 's',
            `estado`    = 'i'
            WHERE `id`  = " . (int)$data->sID . "");
            if ($isUpdate->status) {
               $space = $this->getSpace();
               http_response_code(self::OK);
               return ["status" => true, "message" => "Consumidor inserido", "data" => json_decode($space)];
            }
            http_response_code(self::EXPECTATION_FAILED);
            return ["status" => false, "message" => "Espaço não atribuído ao consumidor"];
         }
         http_response_code(self::EXPECTATION_FAILED);
         return ["status" => false, "message" => "Consumidor não inserido"];
      }
      http_response_code(self::EXPECTATION_FAILED);
      return ["status" => false, "message" => "Consumidor ainda está no espaço"];
   }
}
