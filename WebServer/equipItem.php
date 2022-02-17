<?php
    include "./pdo.php";

    $jsonEcho = new stdClass();

    //post data
    //character_id: 아이템을 장착하는 캐릭터
    //is_weapon: 창착하는 아이템이 장비인가? 아이템인가?
    //item_id: 장착할 아이템의 아이디

    $sql = "";

    if($_POST["is_weapon"] == 1) {
        $sql = "UPDATE `character` SET `weapon_id` = :item_id WHERE `id` = :character_id";
    } else {
        $sql = "UPDATE `character` SET `armor_id` = :item_id WHERE `id` = :character_id";
    }

    
    $pre = $con->prepare($sql);
    $pre->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);
    $pre->bindParam(":character_id", $_POST["character_id"], PDO::PARAM_INT);

    if($pre->execute()) 
    {
        $jsonEcho->success = true;
        echo json_encode($jsonEcho);
    }
    else
    {
        $jsonEcho->success = false;
        $jsonEcho->message = "update fail";
        echo json_encode($jsonEcho);
    }
?>