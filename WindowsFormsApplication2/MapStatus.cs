using System.Collections;
using System.Collections.Generic;
using System.Drawing;


public class MapStatus{

	public int x;
	public int y;
	public int population = 0;
	public bool feed;	//餌
	public bool wall; //壁
	public Color c = Color.White;
	public List<Cellstate> cells = new List<Cellstate>(); //セルの重複。同じ色のセルは入らない（ここに入るセルはすべて違う色）

	public MapStatus(int x, int y, int population, bool feed)
	{
		this.x = x;
		this.y = y;
		this.population = population;
		this.feed = feed;
		this.wall = false;
	}

	public void setFeed(Color c)
	{
		feed = true;
		this.c = c;
	}
	public void delFeed()
	{
		feed = false;
	}
	public void setWall()
	{
		wall = true;
	}
	public void delWall()
	{
		wall = false;
	}
	public void addCell(Cellstate c)
	{
		cells.Add(c);
        MAP.addTotalCell();
	}
	public void delCell(Cellstate c)
	{
		cells.Remove(c);
        MAP.removeTotalCell();
	}
	public void addPopulation()
	{
		population += 1;
	}
	public void reducePopulation()
	{
		population -= 1;
		if (population < 0) population = 0;
	}
	public void addPopulation(int n)
	{
		population += n;
	}
	public void reducePopulation(int n)
	{
		population -= n;
		if (population < 0) population = 0;
	}
}
