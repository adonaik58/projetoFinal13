<?php


include "./models/DB/DBcontroller.php";

class Ticket extends DBController {

    public function GetAllTicket() {
        $resultSelect = (object)$this->select("SELECT * FROM ticket_historico");


        /**
         * Nome do ocupante
         * idade
         * Bilhete de identidade
         * estado 
         * espaço que ocupou
         * Quanto acumulou
         * Nome da marca do carro
         * Nome do modelo do carro
         * cor do carro
         * Mátricula do veículo
         * Data de entrada 
         * Data de saida
         * Tempo que ocupou
         */

        $result = $this->select(
            "SELECT 
                t.id AS id,
                c.id AS c_id,
                c.nome AS name,
                TIMESTAMPDIFF(YEAR, c.idade, NOW()) AS age,
                c.bi AS bi,
                e.estado as status,
                e.id AS space_id,
                e.nome AS s_name,
                ma.nome AS brand,
                mo.nome AS model,
                c.cor_carro AS color,
                c.matricula_carro AS plac,
                c.data_hora_entrada AS date_entrance,
                c.data_hora_saida AS date_out,
                -- TIMESTAMPDIFF(SECOND, c.data_hora_entrada, c.data_hora_saida) AS SecondOcuped,
                TIMESTAMPDIFF(SECOND, c.data_hora_entrada, c.data_hora_saida) / 60 AS minuteOcuped
                FROM ticket_historico t
                LEFT JOIN consumidores c ON c.id = t.id_consumidor 
                LEFT JOIN espacos e ON e.id = t.id_espaco
                LEFT JOIN marcas ma ON ma.id = t.marca
                LEFT JOIN modelos mo ON mo.id = t.modelo
                WHERE
                e.id = t.id_espaco 
            ;
      "
        );
        try {
            $newObject = array();

            foreach ($result as $row) {

                // Calculando o intervalo de tempo
                $data_passada = new DateTime($row->date_entrance ?? "");
                $data_atual   = new DateTime($row->date_out);
                $intervalo    = $data_atual->diff($data_passada);

                $year    = $intervalo->format('%y');
                $month   = $intervalo->format('%m');
                $day     = $intervalo->format('%d');
                $hour    = $intervalo->format('%h');
                $min     = $intervalo->format('%i');
                $sec     = $intervalo->format('%s');

                // // calculando o preço
                $total = (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;

                $newObject[] = [
                    "id"     => $row->id,
                    "c_id"     => $row->c_id,
                    "space_id"  => $row->space_id,
                    "name" => $row->name,
                    "age"   => $row->age,
                    "bi" => $row->bi,
                    "s_name" => $row->s_name,
                    // "estado" => $row->estado,
                    "total" => $total,
                    "brand" => $row->brand,
                    "model" => $row->model,
                    "color" => $row->color,
                    "plac" => $row->plac,
                    "entrance_date"  => $row->date_entrance,
                    "out_date"  => $row->date_out,
                    "time_Ocuped" => [
                        "year"        => (int)$year,
                        "month"        => (int)$month,
                        "day"        => (int)$day,
                        "hour"       => (int)$hour,
                        "minute"     => (int)$min,
                        "secondes"    => (int)$sec,
                    ]

                    // "time_Ocuped" => +$row->minuteOcuped > 60 ? +$row->minuteOcuped / 60 : +$row->minuteOcuped."min",
                ];
            }


            http_response_code(self::OK);
            return json_encode($newObject);
        } catch (\Exception) {
            http_response_code(self::EXPECTATION_FAILED);
            return json_encode(["status" => false, "message" => "Algo deu errado"]);
        }
    }

    public function Fechar() {
        $identificador = (object)self::$data;



        $resultSelect = $this->select(
            "SELECT 
            consumidores.id AS consumer_id,
            e.id AS espacos_id,
            e.bi AS bi,
            consumidores.id_marca_carro AS brand,
            consumidores.id_modelo_carro AS model,
            consumidores.cor_carro AS color,
            consumidores.matricula_carro AS plac,
            consumidores.data_hora_entrada AS data_entrada,
            consumidores.data_hora_saida AS data_saida
            FROM espacos e
            LEFT JOIN consumidores ON e.bi = consumidores.bi 
            LEFT JOIN ticket_historico t ON e.id = t.id_espaco
            WHERE 
            -- e.bi = '$identificador->bi' AND 
            e.id = $identificador->id 
           -- e.estado = 'a' AND 
            -- t.data_saida is NOT NULL
            "
        );

        if (count($resultSelect)) {
            // die(json_encode($resultSelect[count($resultSelect) - 1]));
            $result = (object)$resultSelect[count($resultSelect) - 1];
            // print_r($result);
            // return $result->consumer_id;
            $isUpdated = (object)$this->update("UPDATE espacos SET `bi` = null, `estado` = 'a' WHERE `id` = $identificador->id");
            if ($isUpdated->status) {
                $dateTime = date("Y-m-d H:i:s");
                $isUpdated = (object)$this->update("UPDATE consumidores SET `data_hora_saida` = '$dateTime' WHERE `bi` = '$identificador->bi'");
                if ($isUpdated->status) {

                    // Calculando o intervalo de tempo
                    $data_passada = new DateTime($result->data_entrada ?? "");
                    $data_atual   = new DateTime();
                    $intervalo    = $data_atual->diff($data_passada);


                    $year    = $intervalo->format('%y');
                    $month   = $intervalo->format('%m');
                    $day     = $intervalo->format('%d');
                    $hour    = $intervalo->format('%h');
                    $min     = $intervalo->format('%i');

                    // calculando o preço
                    $preco = (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;
                    $table = 'ticket_historico';
                    $columns = [
                        'id_consumidor',
                        'id_espaco',
                        'montante',
                        'marca',
                        'modelo',
                        'cor',
                        'matricula',
                        'data_entrada',
                        'data_saida'
                    ];
                    self::$data = [
                        $result->consumer_id,
                        $result->espacos_id,
                        $preco,
                        $result->brand,
                        $result->model,
                        $result->color,
                        $result->plac,
                        $result->data_entrada,
                        $dateTime
                    ];

                    $isInserted = (object)$this->insert(
                        $table,
                        $columns
                    );

                    if ($isInserted->status) {
                        http_response_code(self::OK);
                        return ["status" => true, "message" => "Ticket fechado com sucesso"];
                    } else {
                        http_response_code(self::EXPECTATION_FAILED);
                        return ["status" => false, "message" => "Ticket não criado", "data" => self::$data];
                    }
                } else {
                    http_response_code(self::EXPECTATION_FAILED);
                    return ["status" => false, "message" => "Erro ao fechar o consumidor ao espaço"];
                }
            } else {
                http_response_code(self::EXPECTATION_FAILED);
                return ["status" => false, "message" => "Erro ao eliminar o consumidor do espaço"];
            }
        } else {
            http_response_code(self::EXPECTATION_FAILED);
            return ["status" => false, "message" => "Não há espaço atribuído a este consumidor"];
        }
    }
}
