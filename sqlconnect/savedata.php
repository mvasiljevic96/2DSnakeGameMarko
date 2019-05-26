<?php
    $con = mysqli_connect('localhost', 'root', '', 'unityaccess');
    
    //chech that connection happend
    if (mysqli_connect_errno())
    {
        echo "1: Connection failed."; //error code #1 = connection failed
        exit();
    }

    $username = $_POST["name"];
    $newscore = $_POST["score"];

    //double check if there is only one user
    $namecheckquery = "SELECT username from players WHERE username= '" . $username . "';";

    $namecheck = mysqli_query($con, $namecheckquery) or die("2: Name check query failed.");

    if(mysqli_num_rows($namecheck) != 1)
    {
        echo "5: Either no user name with that username or more than 1."; //Error code #5 = number of naes matching =! 1
        exit();
    }

    $updatequery = "UPDATE players SET score = " . $newscore . " WHERE username = '" . $username . "';"; 
    mysqli_query($con, $updatequery) or die("7: Save query failed."); //Error code #7 = update query failed

    echo "0";

?>