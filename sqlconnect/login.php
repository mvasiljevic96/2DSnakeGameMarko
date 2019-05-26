<?php
    $con = mysqli_connect('localhost', 'root', '', 'unityaccess');
    
    //chech that connection happend
    if (mysqli_connect_errno())
    {
        echo "1: Connection failed."; //error code #1 = connection failed
        exit();
    }

    $username = $_POST["name"];
    $password = $_POST["password"];

    //check if name already exists
    $namecheckquery = "SELECT username, salt, hash, score from players WHERE username= '" . $username . "';";

    $namecheck = mysqli_query($con, $namecheckquery) or die("2: Name check query failed."); //error code #2 = namecheckquery failed

    if(mysqli_num_rows($namecheck) != 1)
    {
        echo "5: Either no user name with that username or more than 1."; //Error code #5 = number of naes matching =! 1
        exit();
    }

    //get login info from query
    $existinginifo = mysqli_fetch_assoc($namecheck);

    $salt = $existinginifo["salt"];
    $hash = $existinginifo["hash"];

    $loginhash = crypt($password, $salt);
    if($hash != $loginhash)
    {
        echo "6: Incorrect password."; //Error code #6 = password does not hash to match table
        exit();
    }
    
    echo "0\t" . $existinginifo["score"];

?>