using UnityEngine;

public class IntVect2 {
    public int x;
    public int y;

    public IntVect2(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }

    public override bool Equals(object obj) {
        IntVect2 cls = obj as IntVect2;
        if (cls == null) {
            return false;
        }
        return  (cls == obj);
    }

    public static bool operator ==(IntVect2 a, IntVect2 b){
        return Compare(a,b);
    }

    public static bool operator !=(IntVect2 a, IntVect2 b){
        return !Compare(a,b);
    }

    private static bool Compare(IntVect2 a, IntVect2 b){
        if ((object) a == null && (object) b == null) {
            return true;
        } else if ((object) a == null || (object) b == null) {
            return false;
        } else {
            return a.x == b.x && a.y == b.y;
        }
    }
}
