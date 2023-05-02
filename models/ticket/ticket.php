<?php


include "./models/DB/DBcontroller.php";

class Ticket extends DBController {

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
            WHERE 
            e.bi = '$identificador->bi' AND 
            e.id = '$identificador->id'
            LIMIT 1
            "
        );

        if (count($resultSelect)) {
            $result = (object)$resultSelect[0];
            // print_r($result);
            // return $result->consumer_id;
            $isUpdated = (object)$this->update("UPDATE espacos SET `bi` = null, `estado` = 'a' WHERE `id` = $identificador->id");
            if ($isUpdated->status) {
                $dateTime = date("Y-m-d H:i:s");
                $isUpdated = (object)$this->update("UPDATE consumidores SET `data_hora_saida` = '$dateTime' WHERE `id` = $identificador->id");
                if ($isUpdated->status) {

                    // Calculando o intervalo de tempo
                    $data_passada = new DateTime($result->data_hora_entrada ?? "");
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
                        date("Y-m-d H:i:s")
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
                        return ["status" => false, "message" => "Ticket não criado"];
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
