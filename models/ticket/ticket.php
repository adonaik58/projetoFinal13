<?php


include "./models/DB/DBcontroller.php";

class Ticket extends DBController {

    public function Fechar() {
        $identificador = (object)self::$data;

        $resultSelect = $this->select(
            "SELECT 
            consumidores.id as consumer_id,
            espacos.id as espacos_id,
            espacos.bi_atribuicao as bi,
            consumidores.id_marca_carro as brand,
            consumidores.id_modelo_carro as model,
            consumidores.cor_carro as color,
            consumidores.matricula_carro as plac,
            consumidores.data_hora_entrada as data_entrada,
            consumidores.data_hora_saida as data_saida
            FROM espacos
            LEFT JOIN consumidores ON espacos.bi_atribuicao = consumidores.bi 
            WHERE 
            `espacos.bi_atribuicao` = '$identificador->bi' AND 
            `espacos.id` = '$identificador->id'
            LIMIT 1
            "
        );

        if (count($resultSelect)) {
            $result = (object)$resultSelect;
            return $result->consumer_id;
            $isUpdated = (object)$this->update("UPDATE espacos SET `bi_atribuicao` = '', `ativo` = 's' WHERE `id` = $identificador->id");
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

                    $isInserted = (object)$this->insert("INSERT INTO ticket_historico 
                        (
                            `id_consumidor`,
                            `id_espaco`, 
                            `montante`, 
                            `marca`, 
                            `modelo`, 
                            `cor_carro`,
                            `matricula_carro`, 
                            `data_hora_entrada`,
                            `data_hora_saida`
                        )
                        VALUES 
                        (
                            $result->consumer_id, 
                            $result->espacos_id, 
                            $preco, 
                            $result->brand, 
                            $result->model, 
                            $result->color, 
                            $result->plac, 
                            $result->data_entrada,
                            $result->data_saida
                        );
                    ");

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
