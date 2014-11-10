using System.Collections;
using System.Collections.Generic;

public class Roots{

    public static List<Cellstate> roots = new List<Cellstate>();

    public static void setRootList(Cellstate cs){
		if(cs.parent == null){
			cs.setRoot();
			roots.Add(cs);
		}
		else{
			//print("親がいるのにroot付けてるよ");
		}
	}
    public static void removeRoot(Cellstate cs){
		if(cs.root){
			//root属性を子セルに移す
			foreach(Cellstate cell in cs.children){
				setRootList(cell);
			}
			//削除
			cs.removeRoot();
			roots.Remove(cs);
		}
		else{
			//print("rootじゃないのにroot属性を消そうとしている");
		}
	}
}
