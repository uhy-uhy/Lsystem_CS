using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Drawing;

public class Cellstate {
    public bool root = false;
	public int state;		//自分の満腹度（0になったら餓死）ということにしておく
	public double stateRatio;
	public Color c = Color.Black;
	public bool on_feed = false;	//餌の上にいるかどうか
	public int direction;   //親から見た方向
	public String Lstate;
	public int x,y;
	public List<Cellstate> children = new List<Cellstate>();
	public Cellstate parent = null;
    public bool DEAD = false;
    public int valid = 50;
	//MAP map = new MAP();

	public Cellstate(int state,int x,int y){
		this.state = state;
		this.x = x;
		this.y = y;
	}
	public void setRoot(){
		root = true;
	}
	public void setRatio(int max){
		stateRatio = (double)state/(double)max;
	}
	public void removeRoot(){
		root = false;
	}
	//死滅する際の処理
	public void dead(){
		//子セルの親情報を消す
		//親の居ないセルはrootにする
		if(children.Count > 0　&& root){
			foreach(Cellstate c in children){
				c.removeParent();
			}
		}
		//親セルの子情報から自セルを消す
		if(parent != null)parent.children.Remove(this);
		//子情報を消す
		children.Clear();
		//MAPから生息情報を消す
		//map.Lmap[y][x].reducePopulation();;
		//map.Lmap[y][x].delCell(this);
        MAP.getHashLmap(x, y).reducePopulation();
        MAP.getHashLmap(x, y).delCell(this);
		//rootの場合、rootリストから消える
		if(root) Roots.removeRoot(this);
        DEAD = true;
	}
	
	public void onFeed(){
		on_feed = true;
	}
	public void setpColor(Color c){
		this.c = c;
	}
	public void setdirection(int d){
		this.direction = d;
	}
	public void setParent(Cellstate parent){
		this.parent = parent;
	}
	//親を消す（rootになる）
	public void removeParent(){
        replaceLstate(this.parent.Lstate);
		this.parent = null;
		Roots.setRootList(this);
	}
	public void addChild(Cellstate child){
		this.children.Add(child);
	}
    /*
	public void resetLstate(){
		this.Lstate.Replace(0, Lstate.Length, "");
	}
     * */
	public void addLstate(String s){
		this.Lstate = String.Concat(Lstate,s);
	}
	public void removeChild(int index){
		children.RemoveAt(index);
	}
	public int get_state(){
		return state;
	}
	public void set_state(int state){
		this.state = state;
	}
	public int get_x(){
		return x;
	}
	public int get_y(){
		return y;
	}
	public void set_x(int x){
		this.x = x;
	}
	public void set_y(int y){
		this.y = y;
	}
	public void replaceLstate(String s) {
        this.Lstate = s;
       // this.Lstate.Remove(0,Lstate.Length);
       // this.Lstate = String.Concat(Lstate, s);
		//this.Lstate.replace(0, Lstate.length(), s);
	}

	
}
