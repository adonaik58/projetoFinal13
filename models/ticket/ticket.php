<?php


include "./models/DB/DBcontroller.php";

class Ticket extends DBController {

    public function GetAllTicket(string $filter_value = "", string $order = "") {


        // Verificar se o método recebe parâmetros de filtros válidos
        $filter = "";
        if ($filter || $order) {
            $filter = "ORDER BY " . strtolower($filter_value) . " " . strtoupper($order);
        }

        $getLimit = $this->select("SELECT 
            quant_max_espaco AS quant,
            renda_min AS renda,
            num_hora_gratis AS num
        FROM config");

        // die($filter);
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
                t.data_entrada AS date_entrance,
                t.data_saida AS date_out,
                IF(CAST(TIMESTAMPDIFF(HOUR, t.data_entrada,t.data_saida) AS INT) < " . (int)$getLimit[0]->num . ", 0, FLOOR(TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida) / 60 - (" . (int)$getLimit[0]->num . "*60)) * CAST(c.renda_min AS INT)) AS total,
                TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida) / 60 AS minuteOcuped
                FROM ticket_historico t
                LEFT JOIN consumidores c ON c.id = t.id_consumidor
                LEFT JOIN espacos e ON e.id = t.id_espaco
                LEFT JOIN marcas ma ON ma.id = t.marca
                LEFT JOIN modelos mo ON mo.id = t.modelo
                WHERE
                e.id = t.id_espaco;
                " . $filter . "
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

                // // // calculando o preço
                // $total = (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;

                $newObject[] = [
                    "id"     => $row->id,
                    "c_id"     => $row->c_id,
                    "space_id"  => $row->space_id,
                    "name" => $row->name,
                    "age"   => $row->age,
                    "bi" => $row->bi,
                    "s_name" => $row->s_name,
                    // "estado" => $row->estado,
                    "total" => $row->total,
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
                        "hours"       => (int)$hour,
                        "minutes"     => (int)$min,
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
    public function GetAllTickesStory() {
        // Verificar se o método recebe parâmetros de filtros válidos
        $filter = "";
        $data = (object)self::$data;


        $getLimit = $this->select("SELECT 
            quant_max_espaco AS quant,
            renda_min AS renda,
            num_hora_gratis AS num
        FROM config");

        // die($filter);
        $query = "SELECT 
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
        t.data_entrada AS date_entrance,
        t.data_saida AS date_out,
        IF(CAST(TIMESTAMPDIFF(HOUR, t.data_entrada,t.data_saida) AS INT) < " . (int)$getLimit[0]->num . ", 0, FLOOR(TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida) / 60 - (" . (int)$getLimit[0]->num . "*60)) * CAST(c.renda_min AS INT)) AS total,
        TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida) / 60 AS minuteOcuped
        FROM ticket_historico t
        LEFT JOIN consumidores c ON c.id = t.id_consumidor
        LEFT JOIN espacos e ON e.id = t.id_espaco
        LEFT JOIN marcas ma ON ma.id = t.marca
        LEFT JOIN modelos mo ON mo.id = t.modelo
        WHERE 1
    ";

        $implode = array();
        if (!empty($data->consumer_name)) {
            $implode[] = "c.nome LIKE '%" . $data->consumer_name . "%'";
        }
        if (!empty($data->plac)) {
            $implode[] = "c.matricula_carro = '" . $data->plac . "'";
        }
        if (!empty($data->bi)) {
            $implode[] = "c.bi = '" . $data->bi . "'";
        }
        if (!empty($data->brand)) {
            $implode[] = "ma.id = '" . $data->brand . "'";
        }
        if (!empty($data->model)) {
            $implode[] = "mo.id = '" . $data->model . "'";
        }
        if (!empty($data->code)) {
            $implode[] = "e.nome LIKE = '%" . strtoupper($data->code) . "%'";
        }
        if (!empty($data->plac)) {
            $implode[] = "c.matricula_carro = '" . $data->plac . "'";
        }
        if (!empty($data->space_status)) {
            $implode[] = "e.estado = '" . $data->space_status . "'";
        }
        if (!empty($data->date_entrace)) {
            if ((isset($data->date_entrace) && isset($data->date_outside)) && (strtotime($data->date_entrace) < strtotime($data->date_outside))) {
                $implode[] = " t.data_entrada BETWEEN  '" . $data->date_entrace . "' AND '" . $data->date_outside . "'";
            } else {
                $implode[] = " t.data_entrada = '" . $data->date_entrace . "'";
            }
        }

        if ($implode) {
            $query .= " AND " . implode(" AND ", $implode);
        }

        $query .= " AND e.id = t.id_espaco
         ORDER BY " . $data->order_by . " $data->order " . "
         LIMIT " . $getLimit[0]->quant . ";";
        // die($query);
        $result = $this->select($query);

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

                // // // calculando o preço
                // $total = (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;

                $newObject[] = [
                    "id"     => $row->id,
                    "c_id"     => $row->c_id,
                    "space_id"  => $row->space_id,
                    "name" => $row->name,
                    "age"   => $row->age,
                    "bi" => $row->bi,
                    "s_name" => $row->s_name,
                    // "estado" => $row->estado,
                    "total" => $row->total,
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
                        "hours"       => (int)$hour,
                        "minutes"     => (int)$min,
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
            consumidores.operador AS operer,
            consumidores.id_modelo_carro AS model,
            consumidores.cor_carro AS color,
            consumidores.matricula_carro AS plac,
            consumidores.data_hora_entrada AS data_entrada,
            consumidores.data_hora_saida AS data_saida,
            consumidores.renda_min AS renda
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

                    $breakApartToken = explode(".", $_COOKIE["token"]);
                    $data = (string)"";
                    for ($i = 0; $i < count($breakApartToken); $i++) {
                        if ($i === 2) {
                            $data = json_decode(base64_decode($breakApartToken[$i]));
                        }
                    };

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
                        'data_saida',
                        'renda_min',
                        'operador',
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
                        $dateTime,
                        $result->renda,
                        $data->id
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
