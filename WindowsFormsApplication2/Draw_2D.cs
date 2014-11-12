using System.Collections;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;



/*
 * ルートからすべてノードを順に処理していくver
 * ノードが増えていくと処理時間がかなり長くなる
 * 
 */
class Draw_2D : Form
{
    private int x, y;
    private int wait_time;

    Cellstate now;

    bool wall_flag;
    bool finish = false;
    int init_state = 100;

    public MAP map;
    public Lsystem ls;

    private Image image;

    public Draw_2D() { }
    public Draw_2D(int[,] wall, bool wall_flag, int startX, int startY, int wait_time, int overlap)
    {
        //------------
        // フォームサイズをスクリーンサイズに合わせる処理
        //------------

        // 画面サイズを取得して、それをフォームのサイズとして設定する。
        this.Width = Screen.GetWorkingArea(this).Width / 2;  // フォームの幅を指定
        this.Height = Screen.GetWorkingArea(this).Height / 2;  // フォームの高さを指定
        // フォームの配置位置を指定する。
        int LeftPosition = this.Left;  // フォームの左端位置指定
        int TopPosition = this.Top;    // フォームの上端位置指定
        this.Location = new Point(LeftPosition, TopPosition);

        this.wait_time = wait_time;
        this.wall_flag = wall_flag;
        map = new MAP(this.Height, this.Width);	//MAP作成
        ls = new Lsystem(overlap, init_state);
        x = 0;
        y = 0;


        /*
         * テスト用
         */
        String s = "0";
        //(満腹度、x、y)
        now = new Cellstate(init_state, startY, startX);
        now.setdirection(9);
        now.addLstate(s);
        Roots.setRootList(now);

        /*`
         * 描画用
         */
        //ダブルバッファリング
        SetStyle(
            ControlStyles.DoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint, true
        );

        image = new Bitmap(MAP.LmapX, MAP.LmapY);
        Graphics g = Graphics.FromImage(image);
        Brush brush = new SolidBrush(Color.White);
        g.FillRectangle(brush, 0, 0, image.Width, image.Height);

        /*
         * タイマー
         */
        Timer timer = new Timer();
        timer.Interval = 5;
        timer.Tick += new EventHandler(Test_Tick);
        timer.Start();
    }

    private void Test_Tick(object sender, EventArgs e)
    {
        //グラフィック用
        Graphics g = Graphics.FromImage(image);
        Brush brush;
        //一番古い親・・・・・・・・・・一番若い子の幅優先探索の順番でキューに入れていく
        Queue<Cellstate> tmp_queue = new Queue<Cellstate>();
        //背景を塗りつぶす
        brush = new SolidBrush(Color.White);
        g.FillRectangle(brush, 0, 0, image.Width, image.Height);
        //ルートからスタート
        foreach (Cellstate rc in Roots.roots) tmp_queue.Enqueue(rc);
        Cellstate node;
        while (tmp_queue.Count > 0)
        {
            node = tmp_queue.Dequeue();

            x = node.x;
            y = node.y;
            if (finish == true) node.replaceLstate("d");
            //枝の色を餌の色と同じ色にする
            //if (map.checkFeed(y, x)) node.setpColor(MAP.Lmap[y,x].c);
            if (MAP.checkFeed(x, y)) node.setpColor(MAP.getHashLmap(x, y).c);
            //餌の上に乗ったとき
            if (MAP.checkFeed(x, y))
            {
                node.setpColor(Color.Green);
                node.set_state(init_state); //餌の上は空腹度初期値に
                node.onFeed(); //餌の上にいる get_feedをtrueに
            }
            //描画(表示は後)
            brush = new SolidBrush(node.c);
            g.FillRectangle(brush, node.x, node.y, 1, 1);

            foreach (Cellstate childNode in node.children)
            {
                tmp_queue.Enqueue(childNode); //現セルの子セルをキューの末尾に追加
            }
            /*
             * ここで状態変数を見てルールを適用する
             * 例:0→3|1 , >2→>(3[0]0)
             */
            ls.apply_rule(node); //nullが返ってきた場合、MAP上から消えた（死滅した）

            //nodeがnull →　削除された
            if (node != null)
            {
                /*
                 * ここで式を分割して分割された分-1の子を作成する
                 * 例 3|1 : 3(親)と|1(正面を向いた子)に分ける
                 * 　 3[0]0 : 3(親)と[0(左に45度方向を変えた子)と]0(右に45度方向を変えた子)に分ける
                 */
                if (node.Lstate.Length > 2) node = ls.set_Tree(node);

                //餌の上に乗ってないセルは腹が減る(いらないかも)
                if (!MAP.checkFeed(x, y)) node.state--;
            }



        }
        Invalidate();
        foreach (Cellstate root_cell in Roots.roots)
        {
            ls.checkRoot(root_cell);
        }
        //死滅属性のセル全て削除する
        DelList.delete();
    }

    /*
    * リサイズの検出
    */
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        MAP.LmapX = ClientSize.Width;
        MAP.LmapY = ClientSize.Height;
        image = new Bitmap(MAP.LmapX, MAP.LmapY);
        //MAP.Lmap_htに値が入る前に呼び出されるとエラーが発生する
        if (MAP.Lmap_ht != null) MAP.resizeHashLamp();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.DrawImage(image, 0, 0);
    }


}
