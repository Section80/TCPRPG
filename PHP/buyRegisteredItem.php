<?php
    include "./pdo.php";

    $jsonEcho = new stdClass();

    //POST DATA
    //item_id: 구입할 아이템의 아이디
    //index_in_inventory: 인벤토리 내 몇번째 칸에 받을 것인가? 0 ~ 15
    //character_id: 아이템을 구입한 캐릭터의 아이디
    //price: 구매할 아이템의 가격

    //등록취소 했거나 이미 팔린 아이템인지 확인
    $sql3 = "SELECT COUNT(*) FROM registered_item WHERE `item_id` = :item_id AND `is_sold` = 0";
    $pre3 = $con->prepare($sql3);
    $pre3->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    //COUNT 실패?
    if(!$pre3->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "sql COUNT fail";
        echo json_encode($jsonEcho);
        return;
    }

    $result3 = $pre3->fetchAll(PDO::FETCH_ASSOC);
    $itemNum = $result3[0]["COUNT(*)"];

    //등록 취소 됬거나 이미 팔린 경우
    if($itemNum != 1) 
    {
        $jsonEcho->success = false;
        $jsonEcho->message = "item not exist";
        echo json_encode($jsonEcho);
        return;
    }

    //구입한 registeredItem의 is_sold를 1로 바꾼다.
    $sql = "UPDATE registered_item SET `is_sold` = 1 WHERE `item_id` = :item_id";
    $pre = $con->prepare($sql);

    $pre->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    if(!$pre->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update registered_item table fail";
        echo json_encode($jsonEcho);
        return;
    }

    //구입한 item의 주인을 character_id로 바꾸고 index_in_inventory를 설정한다. 
    $sql2 = "UPDATE item SET `character_id` = :character_id, `index_in_inventory` = :index_in_inventory WHERE `id` = :item_id";
    $pre2 = $con->prepare($sql2);
    $pre2->bindParam(":character_id", $_POST["character_id"], PDO::PARAM_INT);
    $pre2->bindParam(":index_in_inventory", $_POST["index_in_inventory"], PDO::PARAM_INT);
    $pre2->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    if(!$pre2->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update item table fail"; 
        echo json_encode($jsonEcho);
        return;
    }

    //아이템을 구입한 캐릭터의 소지금을 아이템 가격만큼 뺀다.
    $sql4 = "UPDATE `character` SET gold = gold - :price WHERE `id` = :character_id";
    $pre4 = $con->prepare($sql4);
    $pre4->bindParam(":price", $_POST["price"], PDO::PARAM_INT);
    $pre4->bindParam(":character_id", $_POST["character_id"], PDO::PARAM_INT);

    if(!$pre4->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update character gold fail"; 
        echo json_encode($jsonEcho);
        return;
    }

    $jsonEcho->success = true;
    $jsonEcho->index_in_inventory = $_POST["index_in_inventory"];
    $jsonEcho->item_id = $_POST["item_id"];
    echo json_encode($jsonEcho);
    return;
?>