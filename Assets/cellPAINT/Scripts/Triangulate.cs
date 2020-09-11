// COTD Entry submitted by John W. Ratcliff [jratcliff@verant.com]

// ** THIS IS A CODE SNIPPET WHICH WILL EFFICIEINTLY TRIANGULATE ANY
// ** POLYGON/CONTOUR (without holes) AS A STATIC CLASS.  THIS SNIPPET
// ** IS COMPRISED OF 3 FILES, TRIANGULATE.H, THE HEADER FILE FOR THE
// ** TRIANGULATE BASE CLASS, TRIANGULATE.CPP, THE IMPLEMENTATION OF
// ** THE TRIANGULATE BASE CLASS, AND TEST.CPP, A SMALL TEST PROGRAM
// ** DEMONSTRATING THE USAGE OF THE TRIANGULATOR.  THE TRIANGULATE
// ** BASE CLASS ALSO PROVIDES TWO USEFUL HELPER METHODS, ONE WHICH
// ** COMPUTES THE AREA OF A POLYGON, AND ANOTHER WHICH DOES AN EFFICENT
// ** POINT IN A TRIANGLE TEST.
// ** SUBMITTED BY JOHN W. RATCLIFF (jratcliff@verant.com) July 22, 2000

/**********************************************************************/
/************ HEADER FILE FOR TRIANGULATE.H ***************************/
/**********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Triangulate
{
    const float EPSILON = 0.0000000001f;

    // triangulate a contour/polygon, places results in STL vector
    // as series of triangles.
    public static int[] Process(Vector2[] contour){
        /* allocate and initialize list of Vertices in polygon */

        List<int> result = new List<int>();

        int n = contour.Length;
        if (n < 3) return result.ToArray();

        int[] V = new int[n];

        /* we want a counter-clockwise polygon in V */

        if (0.0f < Area(contour))
            for (int v = 0; v < n; v++) V[v] = v;
        else
            for (int v = 0; v < n; v++) V[v] = (n - 1) - v;

        int nv = n;

        /*  remove nv-2 Vertices, creating 1 triangle every time */
        int count = 2 * nv;   /* error detection */

        for (int m = 0, v = nv - 1; nv > 2;)
        {
            /* if we loop, it is probably a non-simple polygon */
            if (0 >= (count--))
            {
                //** Triangulate: ERROR - probable bad polygon!
                return result.ToArray();
            }

            /* three consecutive vertices in current polygon, <u,v,w> */
            int u = v; if (nv <= u) u = 0;     /* previous */
            v = u + 1; if (nv <= v) v = 0;     /* new v    */
            int w = v + 1; if (nv <= w) w = 0;     /* next     */

            if (Snip(contour, u, v, w, nv, V))
            {
                int a, b, c, s, t;

                /* true names of the vertices */
                a = V[u]; b = V[v]; c = V[w];

                /* output Triangle */
                result.Add(c);
                result.Add(b);
                result.Add(a);

                m++;

                /* remove v from remaining polygon */
                for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t]; nv--;

                /* resest error detection counter */
                count = 2 * nv;
            }
        }
        return result.ToArray();
    }

    // compute area of a contour/polygon
    public static float Area(Vector2[] contour)
    {
        int n = contour.Length;

        float A = 0.0f;

        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            A += contour[p].x * contour[q].y - contour[q].x * contour[p].y;
        }
        return A * 0.5f;
    }

    // decide if point Px/Py is inside triangle defined by
    // (Ax,Ay) (Bx,By) (Cx,Cy)
    public static bool InsideTriangle(float Ax, float Ay,
                        float Bx, float By,
                        float Cx, float Cy,
                        float Px, float Py) {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = Cx - Bx; ay = Cy - By;
        bx = Ax - Cx; by = Ay - Cy;
        cx = Bx - Ax; cy = By - Ay;
        apx = Px - Ax; apy = Py - Ay;
        bpx = Px - Bx; bpy = Py - By;
        cpx = Px - Cx; cpy = Py - Cy;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }


    public static bool Snip(Vector2[] contour, int u, int v, int w, int n, int[] V) {
        int p;
        float Ax, Ay, Bx, By, Cx, Cy, Px, Py;

        Ax = contour[V[u]].x;
        Ay = contour[V[u]].y;

        Bx = contour[V[v]].x;
        By = contour[V[v]].y;

        Cx = contour[V[w]].x;
        Cy = contour[V[w]].y;

        if (EPSILON > (((Bx - Ax) * (Cy - Ay)) - ((By - Ay) * (Cx - Ax)))) return false;

        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w)) continue;
            Px = contour[V[p]].x;
            Py = contour[V[p]].y;
            if (InsideTriangle(Ax, Ay, Bx, By, Cx, Cy, Px, Py)) return false;
        }
        return true;
    }
}