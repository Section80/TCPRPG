<?php
    include './pdo.php';
    $jsonEcho = new stdClass();

    //POST DATA
    //item_id: 등록을 취소할 아이템의 아이디
    //index_in_inventory: 인벤토리 내 몇번째 칸에 돌려받을 것인가? 0 ~ 15

    //이미 팔리지 않았는지 확인
    $sql3 = "SELECT COUNT(*) FROM registered_item WHERE `item_id` = :item_id AND `is_sold` = 0";
    $pre3 = $con->prepare($sql3);
    $pre3->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);
    
    //cout fail
    if(!$pre3->execute()){
        $jsonEcho->success = false;
        $jsonEcho->message = "count fail";
        echo json_encode($jsonEcho);
        return;
    }

    $result3 = $pre3->fetchAll(PDO::FETCH_ASSOC);
    $num = $result3[0]["COUNT(*)"];

    //이미 팔려서 실패
    if($num != 1) {
        $jsonEcho->success = false;
        $jsonEcho->message = "already sold";
        echo json_encode($jsonEcho);
        return;     
    }

    //registered item에서 delete한다. 
    $sql = "DELETE FROM registered_item WHERE `item_id` = :item_id";

    $pre = $con->prepare($sql);
    $pre->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    if(!$pre->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "delete fail";
        echo json_encode($jsonEcho);
        return;  
    }

    //item에서 index_in_inventory를 설정한다. 
    $sql2 = "UPDATE item SET `index_in_inventory` = :index_in_inventory WHERE id = :item_id";
    $pre2 = $con->prepare($sql2);
    $pre2->bindParam(":index_in_inventory", $_POST["index_in_inventory"], PDO::PARAM_INT);
    $pre2->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    if(!$pre2->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update fail";
        echo json_encode($jsonEcho);
        return;  
    }

    $jsonEcho->success = true;
    $jsonEcho->item_id = $_POST["item_id"];
    $jsonEcho->index_in_inventory = $_POST["index_in_inventory"];
    echo json_encode($jsonEcho);
    return;  
?>