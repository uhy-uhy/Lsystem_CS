using System.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;

public class Lsystem {

    int overlap; //MAP上に何個までセルを重ねられるか
    int init_state;
    DelList del = new DelList();
    Roots root = new Roots();
	MAP Map = new MAP();

    public Lsystem(int overlap,int init_state)
    {
        this.overlap = overlap;
        this.init_state = init_state;
    }

    String[] cons = { "|", "[", "]", ">", "<", "(", ")", "=" };

    public Cellstate first_proc(Cellstate now,bool finish,int init_state)
    {
        if (finish == true) now.replaceLstate("d");
        //枝の色を餌の色と同じ色にする
        if (MAP.checkFeed(now.x, now.y))
        {
            now.setpColor(MAP.getHashLmap(now.x, now.y).c);
        }
        //餌の上に乗ったとき
        if (MAP.checkFeed(now.x, now.y))
        {
            now.setpColor(Color.Green);
            now.set_state(init_state); //餌の上は空腹度初期値に
            now.onFeed(); //餌の上にいる get_feedをtrueに
        }
        //餌の上に乗ってないセルは腹が減る(いらないかも)
        if (!MAP.checkFeed(now.x, now.y) && now.state >= 0) now.state--;
        //stateが0以下になると死滅状態になる
        if (now.state <= 0)
        {
            now.replaceLstate("d");
        }
        return (now);
    }

    //Lsystem ルールを適用する
    public void apply_rule(Cellstate now)
    {
        String new_s = "";
        String s = "";
        if (now.Lstate.Length > 1)
        {
            s = now.Lstate.Substring(0, 1);
            //移動方向を決める定数consを飛ばす処理
            for (int i = 0; i < cons.Length; i++)
            {
                if (s.Equals(cons[i]))
                {
                    s = now.Lstate.Substring(1, now.Lstate.Length-1);
                    break;
                }
            }
        }
        else if(now.Lstate.Length == 1)s = now.Lstate;

        Random rnd = new Random();
        double margin = (double)MAP.total_cell / (double)init_state;
       // double margin_sig = sigmoid(margin, 4);
        double margin_sig = sigmoid(now.valid/100, 4);
       
        if(s.Equals("0")){
            int ran = rnd.Next(100);
           // if (margin_sig * 10000 >= ran)
            if (now.valid-30 >= ran + 100000)
            {
                new_s = "s";
            }
   
            else if(now.valid-30 < ran && now.valid >= ran )
            {
                if ((ran % 3) == 0) new_s = "3<0>0";
                else if ((ran % 3) == 1) new_s = "3[0]0";
                else new_s = "3[0]0|0";
            }
            
            else
            {
                if ((ran % 3) == 0) new_s = "3|1";
                else new_s = "3|0";
                /*
                if ((ran % 3) == 0) new_s = "3<0>0";
                else if ((ran % 3) == 1) new_s = "3[0]0";
                else new_s = "3[0]0|0";
                //else new_s = "3|0";
                 * */
            }
        }
        else if(s.Equals("1")){
            int ran = rnd.Next(100);
            if ((ran % 2) == 0) new_s = "3[0";
            else new_s = "3]0";
		}
		else if(s.Equals("2")){
            int ran = rnd.Next(100);
            if ((ran % 2) == 0) new_s = "3]0";
            else new_s = "3[0";
		}
		else if(s.Equals("4")){
			new_s = "3<0>0";
		}
        //維持状態
        else if (s.Equals("s"))
        {
            now.state -= 1;

            int ran = rnd.Next(10000);
            double rate = (double)now.state / (double)init_state;
            //double rate_sig = sigmoid(rate, 8);
            double rate_sig = sigmoid((now.valid-50)/100, 8);
            //double margin_sig2 = sigmoid(margin,8);
            //広がり過ぎなので消える
            if ((1 - rate_sig) * 10000 >= ran + 100000000)
            {
                new_s = "d";
            }
            //まだ余裕がある
            else
            {
                new_s = "0";
            }
            if (now.valid < 5)
            {
                new_s = "d";
            }
        }
        //消滅ルール
        else if (s.Equals("d"))
        {

            //先端の子が死んでいく処理
            if (now.children.Count == 0)
            {
                //セルが餌の上にいないとき
                if (now.on_feed == false)
                {

                        //親セルに死滅状態が伝染
                        if (now.parent != null) now.parent.replaceLstate("d");
                        else new_s = s;
                        //now.parent.c = Color.BLUE;
                        //死滅
                        DelList.add(now);
                   
 
                }

                else
                {
                    //ここで餌から成長が再開するかどうか決める
                    //now.replaceLstate("0");
                }
            }
            else
            {
                new_s = s;
            }
        }
        else
        {
            new_s = s;
        }
        
        now.replaceLstate(new_s);
    }


    /* 
     * ルートセルが死滅するかチェック
     */
    public bool checkRoot(Cellstate root_cell)
    {
        //ルートの親が死んでいく処理
        String s = root_cell.Lstate.Substring(0, 1);
        if (s.Equals("d"))
        {
            //子セルが１つ以下の場合はroot属性を子セルに移し、死滅する
            if (root_cell.children.Count <= 1)
            {
                DelList.add(root_cell);
                //total_cell--;
                return true;
            }
        }
        return false;
    }


    /*
	 * 変化した状態に対しての処理
	 * 子を作成し、配置する。（n分木を作成する）
	 */
    public void set_Tree(Cellstate now)
    {
        List<String> Lstr = new List<String>();

        //式を分割してリストに入れる処理
        //現在の仕様だと正しいフォーマット以外が入るとエラーを吐く
		Lstr = split_node(now.Lstate);
		//nowセルの状態変化
		now.replaceLstate(Lstr[0]);

		//子を作る
		//Lstrの１つ目は自分の状態変化　子は２つ目から
		bool child_create = false;
		for (int i = 1; i < Lstr.Count; i++)
		{
			//子の数に応じて子の枝を作る
			Cellstate chi = set_child(now, Lstr[i], 1);
			//chiはnullで帰ってくるかどうか
			if (chi != null)
			{
				now.addChild(chi);
				child_create = true;
			}
		}
		//子セルが1つも生成できない場合
		/*
		 * 子セルを生成する遺伝子（ルール）を持っているが周囲が一杯で生成できない場合と、
		 * 子セルを生成する遺伝子（ルール）を持っていないため生成出来ない場合があるため場合分けする。
		 */
		if (child_create == false)
		{
			//周囲がいっぱいの場合
			if (Lstr.Count > 1)
			{
				now.replaceLstate("d");
			}
			//遺伝子を持っていない場合
			else
			{
				now.replaceLstate("d");
			}
		}
        return ;
    }

	//Lsystem2 子の設定
	/*
	 * 子セルの生成
	 * 
	 */
	public Cellstate set_child(Cellstate now, String Lstr, int num){

		int[] xyd = new int[3];
		/* ここは仕様によって変える必要あり
		 * mark 方向等を指定する定数
		 * var 状態変数
		 * xyd[0] 次に作る子セルのx座標
		 * xyd[1] 次に作る子セルのy座標
		 * xyd[2] 次に作る子セルの向き（親から見て）
		 */
		String mark = Lstr.Substring(0,1);
		String var = Lstr.Substring(1,num);
		xyd = set_locate(now,mark);
		Cellstate child = null;
		

		//mapを見て既に別のセルが配置されていたら子を作成しない  ←今のところ
		//overlap個の重なりを許す
        
		//if(MAP.Lmap[xyd[1],xyd[0]].population < overlap){
        if (MAP.getHashLmap(xyd[0], xyd[1]).population < overlap)
        { 
            //自分と同じ色の枝にぶつかるか見る
            /*
			if(MAP.Lmap[xyd[1],xyd[0]].cells.Count >= 1){
				if(MAP.Lmap[xyd[1],xyd[0]].c == now.c) return null;
				foreach(Cellstate cell in MAP.Lmap[xyd[1],xyd[0]].cells){
					if(cell.c == now.c) return null;
				}
			}
            */
            if (MAP.getHashLmap(xyd[0], xyd[1]).cells.Count >= 1)
            {
                if (MAP.getHashLmap(xyd[0], xyd[1]).c == now.c) return null;
                foreach (Cellstate cell in MAP.getHashLmap(xyd[0], xyd[1]).cells)
                {
                    if (cell.c == now.c) return null;
                }
            }
			//空腹度が-1された子セルを生成
			child = new Cellstate(now.state,(int)xyd[0],(int)xyd[1]);
			//向きの指定
			child.setdirection((int)xyd[2]);
			//親を設定
			child.setParent(now);
			//親の色を遺伝
			child.setpColor(now.c);
			//MAP.Lmap[xyd[1],xyd[0]].addPopulation();
			//MAP.Lmap[xyd[1],xyd[0]].addCell(child);
            MAP.getHashLmap(xyd[0], xyd[1]).addPopulation();
            MAP.getHashLmap(xyd[0], xyd[1]).addCell(child);
			//子の状態変数を設定
			child.addLstate(String.Concat(mark,var));
			//空腹度が0以下の子セルが生成されたときは子セルを死滅状態に
			if(child.state <= 0){
				child.replaceLstate("d");
				return child;
			}

		}
		return child;

	}

	/* 
	 * 親のdirectionによる子供の配置位置算出　ついでに方向も
	 * 返り値は子の位置、向き
	 * 
	 */
	public int[] set_locate(Cellstate now,String s){
		int[] xyd = new int[3];
		int direction = set_direction(now,s);
		int x=0,y=0;
		if(direction == 1) {
			x = -1;
			y = 1;
		}else if(direction == 2){
			x = 0;
			y = 1;
		}else if(direction == 3){
			x = 1;
			y = 1;
		}else if(direction == 4){
			x = -1;
			y = 0;
		}else if(direction == 5){
			x = 0;
			y = 0;
		}else if(direction == 6){
			x = 1;
			y = 0;
		}else if(direction == 7){
			x = -1;
			y = -1;
		}else if(direction == 8){
			x = 0;
			y = -1;
		}else if(direction == 9){
			x = 1;
			y = -1;
		}
		xyd[0] = now.x + x;
		xyd[1] = now.y + y;
		//壁に当たったときの処理
		if (xyd[0] < 0) xyd[0] = 0;
		if (xyd[0] >= MAP.LmapX) xyd[0] = MAP.LmapX - 1;
		if (xyd[1] < 0) xyd[1] = 0;
		if (xyd[1] >= MAP.LmapY) xyd[1] = MAP.LmapY - 1;
		//if (MAP.Lmap[xyd[1],xyd[0]].wall)
        if (MAP.getHashLmap(xyd[0],xyd[1]).wall)
		{
			xyd[0] = xyd[0] - x;
			xyd[1] = xyd[1] - y;
		}

		xyd[2] = direction;

		return xyd;
	}

	/* 
	 * 方向の割り当て
	 *
	 */
	public int set_direction(Cellstate now, String d)
	{
		int now_d = now.direction;
		int next_d = 0;
		if (now_d == 1)
		{
			if (d.Equals("]")) next_d = 4;
			else if (d.Equals("[")) next_d = 2;
			else if (d.Equals(">")) next_d = 7;
			else if (d.Equals("<")) next_d = 3;
			else if (d.Equals("(")) next_d = 6;
			else if (d.Equals(")")) next_d = 8;
			else if (d.Equals("=")) next_d = 9;
			else next_d = 1;
		}
		if (now_d == 2)
		{
			if (d.Equals("]")) next_d = 1;
			else if (d.Equals("[")) next_d = 3;
			else if (d.Equals(">")) next_d = 4;
			else if (d.Equals("<")) next_d = 6;
			else if (d.Equals("(")) next_d = 9;
			else if (d.Equals(")")) next_d = 7;
			else if (d.Equals("=")) next_d = 8;
			else next_d = 2;
		}
		if (now_d == 3)
		{
			if (d.Equals("]")) next_d = 2;
			else if (d.Equals("[")) next_d = 6;
			else if (d.Equals(">")) next_d = 1;
			else if (d.Equals("<")) next_d = 9;
			else if (d.Equals("(")) next_d = 8;
			else if (d.Equals(")")) next_d = 4;
			else if (d.Equals("=")) next_d = 7;
			else next_d = 3;
		}
		if (now_d == 4)
		{
			if (d.Equals("]")) next_d = 7;
			else if (d.Equals("[")) next_d = 1;
			else if (d.Equals(">")) next_d = 8;
			else if (d.Equals("<")) next_d = 2;
			else if (d.Equals("(")) next_d = 3;
			else if (d.Equals(")")) next_d = 9;
			else if (d.Equals("=")) next_d = 6;
			else next_d = 4;
		}
		if (now_d == 5)
		{
			System.Random rnd = new System.Random();
			int ran = rnd.Next(4);
			if (ran == 0) next_d = 2;
			else if (ran == 1) next_d = 4;
			else if (ran == 2) next_d = 6;
			else next_d = 8;
		}
		if (now_d == 6)
		{
			if (d.Equals("]")) next_d = 3;
			else if (d.Equals("[")) next_d = 9;
			else if (d.Equals(">")) next_d = 2;
			else if (d.Equals("<")) next_d = 8;
			else if (d.Equals("(")) next_d = 7;
			else if (d.Equals(")")) next_d = 1;
			else if (d.Equals("=")) next_d = 4;
			else next_d = 6;
		}
		if (now_d == 7)
		{
			if (d.Equals("]")) next_d = 8;
			else if (d.Equals("[")) next_d = 4;
			else if (d.Equals(">")) next_d = 9;
			else if (d.Equals("<")) next_d = 1;
			else if (d.Equals("(")) next_d = 2;
			else if (d.Equals(")")) next_d = 6;
			else if (d.Equals("=")) next_d = 3;
			else next_d = 7;
		}
		if (now_d == 8)
		{
			if (d.Equals("]")) next_d = 9;
			else if (d.Equals("[")) next_d = 7;
			else if (d.Equals(">")) next_d = 6;
			else if (d.Equals("<")) next_d = 4;
			else if (d.Equals("(")) next_d = 1;
			else if (d.Equals(")")) next_d = 3;
			else if (d.Equals("=")) next_d = 2;
			else next_d = 8;
		}
		if (now_d == 9)
		{
			if (d.Equals("]")) next_d = 6;
			else if (d.Equals("[")) next_d = 8;
			else if (d.Equals(">")) next_d = 3;
			else if (d.Equals("<")) next_d = 7;
			else if (d.Equals("(")) next_d = 4;
			else if (d.Equals(")")) next_d = 2;
			else if (d.Equals("=")) next_d = 1;
			else next_d = 9;
		}

		return next_d;
	}

	/*
	 * 文字列を定数毎に分割する
	 * 
	 */
	public List<String> split_node(String s)
	{
		List<String> ls = new List<String>();
		List<int> order_list = new List<int>();
		int order = 0;
		order_list.Add(0);
		order_list.Add(s.Length);
		for (int i = 0; i < cons.Length; i++)
		{
			while ((order = s.IndexOf(cons[i], order)) != -1)
			{
				order_list.Add(order);
				order++;
			}
			order = 0;
		}
		order_list.Sort();
		for (int i = 0; i < order_list.Count - 1; i++)
		{
			ls.Add(s.Substring(order_list[i], (order_list[i + 1]) - order_list[i]));
		}
		return ls;
	}

    //xが０～１のシグモイド関数
    double sigmoid(double x, double gain)
    {
        return 1.0 / (1.0 + Math.Exp(-gain * (x * 2 - 1)));
    }

}
 