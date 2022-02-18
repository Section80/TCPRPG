<?php

    include "./pdo.php";

    $jsonEcho = new stdClass();

    //POST DATA
    //characterID: 아이템 목록을 가져올 캐릭터의 ID

    $sql = "SELECT a.id, a.index_in_inventory, a.name, a.is_weapon, a.job, a.level_limit, a.strong, a.dexility, a.intellect, a.luck FROM `item` a WHERE a.character_id = :character_id";

    $pre = $con->prepare($sql);
    $characterID = $_POST["characterID"];
    $pre->bindParam(":character_id", $characterID, PDO::PARAM_INT);

    if($pre->execute()) {
        $result = $pre->fetchAll(PDO::FETCH_ASSOC);

        $jsonEcho->success = true;
        $jsonEcho->data = $result;
    }
    else {
        $jsonEcho->success = false;
        $jsonEcho->message = "sql fail";
    }

    echo json_encode($jsonEcho);
?>
