<?php

include "./models/DB/DBcontroller.php";


class Dashboard extends DBController {

    public function getDashboardResults(int $var = null) {
        return [
            "totalPayment"      => $this->FacturationService(),
            "spaceOpen"         => $this->FreeSpace(),
            "numCarEntrance"    => $this->carEntrance(),
            "ticketClosed"      => $this->ticketClosed(),
            "spaceOcuped"       => $this->spaceOcuped(),
            "maxEarnToday"      => (int)$this->maxEarnToday(),
            "avgTimeStay"       => (int)$this->avgTimeStay(),
        ];
    }

    private function FacturationService() {
        try {
            $db = new DBController;
            $getLimit = $db->select("SELECT 
            quant_max_espaco AS quant,
            renda_min AS renda,
            num_hora_gratis AS num
        FROM config");
            // return $getLimit[0]->num;


            $result = $db->select("SELECT 
                t.data_entrada as i,
                t.data_saida as f,
                LAST_DAY(CAST(t.data_entrada AS Date)) AS lastDay,
                IF(CAST(TIMESTAMPDIFF(HOUR, t.data_entrada,t.data_saida) AS INT) < " . (int)$getLimit[0]->num . ", 0, FLOOR(TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida) / 60 - (" . (int)$getLimit[0]->num . "*60)) * CAST(t.renda_min AS INT)) AS total
                FROM ticket_historico t;
            ");




            // if ($result->execute()) {
            $preco = 0;
            foreach ($result as $row) {

                // Calculando o intervalo de tempo
                $data_passada = new DateTime($row->i ?? "");
                $data_atual   = new DateTime($row->f ?? "");
                $intervalo    = $data_atual->diff($data_passada);


                // $year    = $intervalo->format('%y');
                // $month   = $intervalo->format('%m');
                // $day     = $intervalo->format('%d');
                // $hour    = $intervalo->format('%h');
                // $min     = $intervalo->format('%i');
                // $seg     = $intervalo->format('%s');

                // calculando o preço
                // $preco += (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;
                // $lastDay = explode("-", $row->lastDay)[2];
                $preco += $row->total;
            }
            http_response_code(self::OK);
            return ($preco);
            // } else {
            //     http_response_code(self::EXPECTATION_FAILED);
            //     return (["status" => false, "message" => "Algo deu errado ao pegar o total"]);
            // }
        } catch (\Exception) {
            http_response_code(self::EXPECTATION_FAILED);
            return (0);
            // return (["status" => false, "message" => "Algo deu errado"]);
        }
    }
    private function FreeSpace() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM espacos e
        WHERE e.estado = 'a'");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function spaceOcuped() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM espacos e
        WHERE e.estado = 'i'");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function carEntrance() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM consumidores");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function ticketClosed() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM ticket_historico");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function avgTimeStay() {
        $result = $this->connection->query("SELECT AVG(TIMESTAMPDIFF(MINUTE, t.data_entrada, t.data_saida)) AS maxTime
        FROM ticket_historico t;
        ");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->maxTime;
    }
    private function maxEarnToday() {
        $result = $this->connection->query("SELECT 
        FLOOR((MAX(TIMESTAMPDIFF(SECOND, t.data_entrada, t.data_saida))) / 60) * CAST(10 AS INT) AS maxEarn
        FROM ticket_historico t
        ;");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->maxEarn;
    }
}
