using System.Collections;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

/*
 * ルートからすべてのノードを順に処理するのではなく、先端のノードののみを処理するver
 * 描画は毎フレーム毎に背景で塗りつぶすのではなく、前フレームの描画に足していく感じ
 * 
 */
class Draw_2D_2 : Form
{
    private int x, y;
    private int wait_time;

    private Cellstate now;

    bool wall_flag;
    bool finish = false;
    int init_state = 300;

    public MAP map;
    public Lsystem ls;

    private Image image;
    private bool resize = false;

    bool mouse_down_flag = false;
    //Point mouse_p = null;

    private Queue<Cellstate> tip_queue = new Queue<Cellstate>();
    private Queue<Cellstate> new_root = new Queue<Cellstate>();

    public Draw_2D_2() { }
    public Draw_2D_2(int[,] wall, bool wall_flag, int startX, int startY, int wait_time, int overlap)
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
        ls = new Lsystem(overlap,init_state);
        x = 0;
        y = 0;

        

        /*
         * テスト用
         */
        String s = "0";
        //(満腹度、x、y)
        now = CreateNewCell(s,init_state,startX,startY,5);
        tip_queue.Enqueue(now);

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


        int feed_x = 100;
        int feed_y = 100;
        int feed_width = 40;
        int feed_height = 40;
        Brush brush_feed = new SolidBrush(Color.Red);
        g.FillRectangle(brush_feed, feed_x, feed_y, feed_width, feed_height);
        //餌の配置
        for (int i = feed_y; i < feed_y+feed_height; i++)
        {
            for (int j = feed_x; j < feed_x+feed_width; j++)
            {
               // MAP.addFeedMap(i, j, Color.Red);
            }
        }

        /*
         * マウスイベントにデリゲートを追加
         */
        this.MouseDown += new MouseEventHandler(MouseDownIvent);
        this.MouseUp += new MouseEventHandler(MouseUpIvent);
        this.MouseMove += new MouseEventHandler(MouseMoveIvent);

        /*
         * タイマー
         */
        Timer timer = new Timer();
        timer.Interval = 10;
        timer.Tick += new EventHandler(Test_Tick);
        timer.Start();
    }

    int[] count = new int[3];

    private void Test_Tick(object sender, EventArgs e)
    {
        count[0] = 0;
        count[1] = 0;
        count[2] = 0;
        //リサイズされた場合の処理
        if (resize)
        {
            MAP.LmapX = ClientSize.Width;
            MAP.LmapY = ClientSize.Height;
            Image new_image = new Bitmap(MAP.LmapX, MAP.LmapY);
            Graphics new_g = Graphics.FromImage(new_image);
            Brush new_brush = new SolidBrush(Color.White);
            new_g.FillRectangle(new_brush, 0, 0, new_image.Width, new_image.Height);

            int pre_width = (new_image.Width >= image.Width) ? image.Width : new_image.Width;
            int pre_height = (new_image.Height >= image.Height) ? image.Height : new_image.Height;
            Bitmap bm = new Bitmap(new_image);
            Bitmap pre_bm = new Bitmap(image);
            for (int y = 0; y < pre_height; y++)
            {
                for (int x = 0; x < pre_width; x++)
                {
                    Color c = pre_bm.GetPixel(x, y);
                    bm.SetPixel(x, y, c);
                }
            }
            image = new Bitmap(bm);

            MAP.resizeHashLamp();

            resize = false;
        }
        //グラフィック用
        Graphics g = Graphics.FromImage(image);
        Brush brush;

        //新しいrootの追加
        while (new_root.Count > 0)
        {
            tip_queue.Enqueue(new_root.Dequeue());

        }
        List<Cellstate> new_node = new List<Cellstate>(); //先端ノードリスト
        Cellstate node;

        //先端のノードだけを処理する
        while (tip_queue.Count > 0)
        {
            node = tip_queue.Dequeue();

            Cellstate parent = null;
            if (node.parent != null) parent = node.parent;

            x = node.x;
            y = node.y;

      
            //ノードへの初期操作
            ls.first_proc(node, finish, init_state);

            //描画(表示は後)
            brush = new SolidBrush(node.c);
            g.FillRectangle(brush, node.x, node.y, 1, 1);

            if (node.root && node.children.Count > 0) continue;
            /*
             * ここで状態変数を見てルールを適用する
             * 例:0→3|1 , >2→>(3[0]0)
             */
            ls.apply_rule(node);

            //子ノードの追加
            if (node != null)
            {
                /*
                 * ここで式を分割して分割された分-1の子を作成する
                 * 例 3|1 : 3(親)と|1(正面を向いた子)に分ける
                 * 　 3[0]0 : 3(親)と[0(左に45度方向を変えた子)と]0(右に45度方向を変えた子)に分ける
                 */
                if (node.Lstate.Length > 2) ls.set_Tree(node);

                //餌の上に乗ってないセルは腹が減る(いらないかも)
                //if (!map.checkFeed(y, x)) node.state--;
                //子ノードを生成することができている場合
                if (node.children.Count > 0)
                {
                    foreach (Cellstate childNode in node.children)
                    {
                        new_node.Add(childNode); //現ノードの子ノードを追加
                        count[1]++;
                    }
                }
                //子ノードを生成できていない場合
                else
                {
                    //Console.WriteLine(node.x+" , "+ node.y);
                    new_node.Add(node); //現ノードを追加
                    count[2]++;
                }
            }
        }
        foreach (Cellstate root_cell in Roots.roots)
        {
            //rootノードが死滅しない
            if (ls.checkRoot(root_cell) == false)
            {
                new_node.Add(root_cell);
            }
            //rootノードが死滅する
            else
            {
                new_node.Add(root_cell);
            }
        }

        foreach (Cellstate cs in DelList.queue)
        {
            //背景色で描画(ノードの色を消す)
            brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, cs.x, cs.y, 1, 1);
            cs.dead();
            //現ノードの親ノードが先端（子ノードが0）のとき、先端ノードリストに追加
            if (cs.parent != null)
            {
                if (cs.parent.children.Count == 0)
                {
                    new_node.Add(cs.parent);
                    count[0]++;
                }

            }
        }

        //死滅属性のセル全て削除する
        DelList.clear();
        /*
        Console.WriteLine("死滅して先端になったノード" + count[0]
                            + "\n生成した子の数" + count[1]
                            + "\n生成できなった先端ノード" + count[2]);
         * */

        //先端ノードキューの更新
        foreach (Cellstate cs in new_node)
        {
            if(cs.DEAD == false) tip_queue.Enqueue(cs);
        }

        Invalidate();

    }

    /*
     * リサイズの検出
     */
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        //プログラム開始時に1回呼ばれるようなので、そこはスルーする
        if (MAP.Lmap_ht != null) resize = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.DrawImage(image, 0, 0);
    }

    private void MouseDownIvent(object sender, MouseEventArgs e)
    {
        String new_str = "0";
        Cellstate new_cs;
        //X座標を取得する
        int x = e.X;
        //Y座標を取得する
        int y = e.Y;
        //mouse_p.X = x;
        //mouse_p.Y = y;
        //(満腹度、x、y)
        new_cs = new Cellstate(init_state, x, y);
        new_cs.setdirection(9);
        new_cs.addLstate(new_str);
        Roots.setRootList(new_cs);
        new_root.Enqueue(new_cs);

        mouse_down_flag = true;
    }

    private void MouseUpIvent(object sender, MouseEventArgs e)
    {

        //X座標を取得する
        int x = e.X;
        //Y座標を取得する
        int y = e.Y;

        mouse_down_flag = false;
    }

    private void MouseMoveIvent(object sender, MouseEventArgs e)
    {
        if (mouse_down_flag)
        {
            //X座標を取得する
            int x = e.X;
            //Y座標を取得する
            int y = e.Y;

            Cellstate new_cs = CreateNewCell("0",init_state,x,y,5);
            new_root.Enqueue(new_cs);
        }
    }

    private Cellstate CreateNewCell(String Lstate,int init_state,int x,int y,int direction)
    {
        Cellstate new_cell = new Cellstate(init_state,x,y);
        new_cell.setdirection(direction);
        new_cell.addLstate(Lstate);
        Roots.setRootList(new_cell);

        MAP.getHashLmap(x, y).addPopulation();
        MAP.getHashLmap(x, y).addCell(new_cell);

        return (new_cell);
    }

}
