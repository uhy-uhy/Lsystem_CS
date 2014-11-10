using System.Collections;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;


public class Run_Lsystem
{

    static void Main()
    {

        int startX = 230;
        int startY = 235;

        //描画間隔
        int wait_time = 0;
        //MAP上に何個までセルの重なりを許すか(別色のセルの重なりを許すかどうかで同色は許さない)
        int overlap = 1;
        //壁を作るかどうか
        bool wall = false;
        int[,] wallNum = new int[20, 20];

        //Application.Run(new Draw_2D(wallNum, wall, startX, startY, wait_time, overlap));
        Application.Run(new Draw_2D_2(wallNum, wall, startX, startY, wait_time, overlap));
    }
}

