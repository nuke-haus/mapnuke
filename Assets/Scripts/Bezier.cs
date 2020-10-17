// Bezier.cs
//
// Implementations for splines and paths with various degrees of smoothness. A 'path', or 'spline', is arbitrarily long
// and may be composed of smaller path sections called 'curves'. For example, a Bezier path is made from multiple
// Bezier curves.
//
// Regarding naming, the word 'spline' refers to any path that is composed of piecewise parts. Strictly speaking one
// could call a composite of multiple Bezier curves a 'Bezier Spline' but it is not a common term. In this file the
// word 'path' is used for a composite of Bezier curves.
//
// Copyright (c) 2006, 2017 Tristan "Poopypants" Grimmer.
// Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby
// granted, provided that the above copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
// INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN
// AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
// PERFORMANCE OF THIS SOFTWARE.

using UnityEngine;
using UnityEngine.Assertions;

// A CubicBezierCurve represents a single segment of a Bezier path. It knows how to interpret 4 CVs using the Bezier basis
// functions. This class implements cubic Bezier curves -- not linear or quadratic.
internal struct CubicBezierCurve
{
    private Vector3 controlVerts_0;
    private Vector3 controlVerts_1;
    private Vector3 controlVerts_2;
    private Vector3 controlVerts_3;

    public CubicBezierCurve(Vector3 controlVerts_1, Vector3 controlVerts_2, Vector3 controlVerts_3, Vector3 controlVerts_4)
    {
        controlVerts_0 = controlVerts_1;
        this.controlVerts_1 = controlVerts_2;
        this.controlVerts_2 = controlVerts_3;
        this.controlVerts_3 = controlVerts_4;
    }

    public Vector3 GetPoint(float t)                            // t E [0, 1].
    {
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));
        var c = 1.0f - t;

        // The Bernstein polynomials.
        var bb0 = c * c * c;
        var bb1 = 3 * t * c * c;
        var bb2 = 3 * t * t * c;
        var bb3 = t * t * t;

        var point = controlVerts_0 * bb0 + controlVerts_1 * bb1 + controlVerts_2 * bb2 + controlVerts_3 * bb3;
        return point;
    }

    public Vector3 GetTangent(float t)                          // t E [0, 1].
    {
        // See: http://bimixual.org/AnimationLibrary/beziertangents.html
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));

        var q0 = controlVerts_0 + ((controlVerts_1 - controlVerts_0) * t);
        var q1 = controlVerts_1 + ((controlVerts_2 - controlVerts_1) * t);
        var q2 = controlVerts_2 + ((controlVerts_3 - controlVerts_2) * t);

        var r0 = q0 + ((q1 - q0) * t);
        var r1 = q1 + ((q2 - q1) * t);
        var tangent = r1 - r0;
        return tangent;
    }

    public float GetClosestParam(Vector3 pos, float paramThreshold = 0.000001f)
    {
        return GetClosestParamRec(pos, 0.0f, 1.0f, paramThreshold);
    }

    private float GetClosestParamRec(Vector3 pos, float beginT, float endT, float thresholdT)
    {
        var mid = (beginT + endT) / 2.0f;

        // Base case for recursion.
        if ((endT - beginT) < thresholdT)
            return mid;

        // The two halves have param range [start, mid] and [mid, end]. We decide which one to use by using a midpoint param calculation for each section.
        var paramA = (beginT + mid) / 2.0f;
        var paramB = (mid + endT) / 2.0f;

        var posA = GetPoint(paramA);
        var posB = GetPoint(paramB);
        var distASq = (posA - pos).sqrMagnitude;
        var distBSq = (posB - pos).sqrMagnitude;

        if (distASq < distBSq)
            endT = mid;
        else
            beginT = mid;

        // The (tail) recursive call.
        return GetClosestParamRec(pos, beginT, endT, thresholdT);
    }
}


// A CubicBezierPath is made of a collection of cubic Bezier curves. If two points are supplied they become the end
// points of one CubicBezierCurve and the 2 interior CVs are generated, creating a small straight line. For 3 points
// the middle point will be on both CubicBezierCurves and each curve will have equal tangents at that point.
public class CubicBezierPath
{
    public enum Type
    {
        Open,
        Closed
    }

    private Type type = Type.Open;
    private int numCurveSegments = 0;
    private int numControlVerts = 0;
    private Vector3[] controlVerts = null;

    // The term 'knot' is another name for a point right on the path (an interpolated point). With this constructor the
    // knots are supplied and interpolated. knots.length (the number of knots) must be >= 2. Interior Cvs are generated
    // transparently and automatically.
    public CubicBezierPath(Vector3[] knots, Type t = Type.Open) { InterpolatePoints(knots, t); }
    public Type GetPathType() { return type; }
    public bool IsClosed() { return (type == Type.Closed) ? true : false; }
    public bool IsValid() { return (numCurveSegments > 0) ? true : false; }
    public void Clear()
    {
        controlVerts = null;
        type = Type.Open;
        numCurveSegments = 0;
        numControlVerts = 0;
    }

    // A closed path will have one more segment than an open for the same number of interpolated points.
    public int GetNumCurveSegments() { return numCurveSegments; }
    public float GetMaxParam() { return (float)numCurveSegments; }

    // Access to the raw CVs.
    public int GetNumControlVerts() { return numControlVerts; }
    public Vector3[] GetControlVerts() { return controlVerts; }

    public float ComputeApproxLength()
    {
        if (!IsValid())
            return 0.0f;

        // For a closed path this still works if you consider the last point as separate from the first. That is, a closed
        // path is just like an open except the last interpolated point happens to match the first.
        var numInterpolatedPoints = numCurveSegments + 1;
        if (numInterpolatedPoints < 2)
            return 0.0f;

        var totalDist = 0.0f;
        for (var n = 1; n < numInterpolatedPoints; n++)
        {
            var a = controlVerts[(n - 1) * 3];
            var b = controlVerts[n * 3];
            totalDist += (a - b).magnitude;
        }

        if (totalDist == 0.0f)
            return 0.0f;

        return totalDist;
    }

    public float ComputeApproxParamPerUnitLength()
    {
        var length = ComputeApproxLength();
        return (float)numCurveSegments / length;
    }

    public float ComputeApproxNormParamPerUnitLength()
    {
        var length = ComputeApproxLength();
        return 1.0f / length;
    }

    // Interpolates the supplied points. Internally generates any necessary CVs. knots.length (number of knots)
    // must be >= 2.
    public void InterpolatePoints(Vector3[] knots, Type t)
    {
        var numKnots = knots.Length;
        Assert.IsTrue(numKnots >= 2);
        Clear();
        type = t;
        switch (type)
        {
            case Type.Open:
                {
                    numCurveSegments = numKnots - 1;
                    numControlVerts = 3 * numKnots - 2;
                    controlVerts = new Vector3[numControlVerts];

                    // Place the interpolated CVs.
                    for (var n = 0; n < numKnots; n++)
                        controlVerts[n * 3] = knots[n];

                    // Place the first and last non-interpolated CVs.
                    var initialPoint = (knots[1] - knots[0]) * 0.25f;

                    // Interpolate 1/4 away along first segment.
                    controlVerts[1] = knots[0] + initialPoint;
                    var finalPoint = (knots[numKnots - 2] - knots[numKnots - 1]) * 0.25f;

                    // Interpolate 1/4 backward along last segment.
                    controlVerts[numControlVerts - 2] = knots[numKnots - 1] + finalPoint;

                    // Now we'll do all the interior non-interpolated CVs.
                    for (var k = 1; k < numCurveSegments; k++)
                    {
                        var a = knots[k - 1] - knots[k];
                        var b = knots[k + 1] - knots[k];
                        var aLen = a.magnitude;
                        var bLen = b.magnitude;

                        if ((aLen > 0.0f) && (bLen > 0.0f))
                        {
                            var abLen = (aLen + bLen) / 8.0f;
                            var ab = (b / bLen) - (a / aLen);
                            ab.Normalize();
                            ab *= abLen;

                            controlVerts[k * 3 - 1] = knots[k] - ab;
                            controlVerts[k * 3 + 1] = knots[k] + ab;
                        }
                        else
                        {
                            controlVerts[k * 3 - 1] = knots[k];
                            controlVerts[k * 3 + 1] = knots[k];
                        }
                    }
                    break;
                }

            case Type.Closed:
                {
                    numCurveSegments = numKnots;

                    // We duplicate the first point at the end so we have contiguous memory to look of the curve value. That's
                    // what the +1 is for.
                    numControlVerts = 3 * numKnots + 1;
                    controlVerts = new Vector3[numControlVerts];

                    // First lets place the interpolated CVs and duplicate the first into the last CV slot.
                    for (var n = 0; n < numKnots; n++)
                        controlVerts[n * 3] = knots[n];

                    controlVerts[numControlVerts - 1] = knots[0];

                    // Now we'll do all the interior non-interpolated CVs. We go to k=NumCurveSegments which will compute the
                    // two CVs around the zeroth knot (points[0]).
                    for (var k = 1; k <= numCurveSegments; k++)
                    {
                        var modkm1 = k - 1;
                        var modkp1 = (k + 1) % numCurveSegments;
                        var modk = k % numCurveSegments;

                        var a = knots[modkm1] - knots[modk];
                        var b = knots[modkp1] - knots[modk];
                        var aLen = a.magnitude;
                        var bLen = b.magnitude;
                        var mod3km1 = 3 * k - 1;

                        // Need the -1 so the end point is a duplicated start point.
                        var mod3kp1 = (3 * k + 1) % (numControlVerts - 1);
                        if ((aLen > 0.0f) && (bLen > 0.0f))
                        {
                            var abLen = (aLen + bLen) / 8.0f;
                            var ab = (b / bLen) - (a / aLen);
                            ab.Normalize();
                            ab *= abLen;

                            controlVerts[mod3km1] = knots[modk] - ab;
                            controlVerts[mod3kp1] = knots[modk] + ab;
                        }
                        else
                        {
                            controlVerts[mod3km1] = knots[modk];
                            controlVerts[mod3kp1] = knots[modk];
                        }
                    }
                    break;
                }
        }
    }

    // For a closed path the last CV must match the first.
    public void SetControlVerts(Vector3[] cvs, Type t)
    {
        var numCVs = cvs.Length;
        Assert.IsTrue(numCVs > 0);
        Assert.IsTrue(((t == Type.Open) && (numCVs >= 4)) || ((t == Type.Closed) && (numCVs >= 7)));
        Assert.IsTrue(((numCVs - 1) % 3) == 0);
        Clear();
        type = t;

        numControlVerts = numCVs;
        numCurveSegments = ((numCVs - 1) / 3);
        controlVerts = cvs;
    }

    // t E [0, numSegments]. If the type is closed, the number of segments is one more than the equivalent open path.
    public Vector3 GetPoint(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float)numCurveSegments;

            while (t > (float)numCurveSegments)
                t -= (float)numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float)numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float)numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        var segment = (int)t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        var bc = new CubicBezierCurve(controlVerts[3 * segment + 0], controlVerts[3 * segment + 1], controlVerts[3 * segment + 2], controlVerts[3 * segment + 3]);
        return bc.GetPoint(t - (float)segment);
    }

    // Does the same as GetPoint except that t is normalized to be E [0, 1] over all segments. The beginning of the curve
    // is at t = 0 and the end at t = 1. Closed paths allow a value bigger than 1 in which case they loop.
    public Vector3 GetPointNorm(float t)
    {
        return GetPoint(t * (float)numCurveSegments);
    }

    // Similar to GetPoint but returns the tangent at the specified point on the path. The tangent is not normalized.
    // The longer the tangent the 'more influence' it has pulling the path in that direction.
    public Vector3 GetTangent(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float)numCurveSegments;

            while (t > (float)numCurveSegments)
                t -= (float)numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float)numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float)numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        var segment = (int)t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        var bc = new CubicBezierCurve(controlVerts[3 * segment + 0], controlVerts[3 * segment + 1], controlVerts[3 * segment + 2], controlVerts[3 * segment + 3]);
        return bc.GetTangent(t - (float)segment);
    }

    public Vector3 GetTangentNorm(float t)
    {
        return GetTangent(t * (float)numCurveSegments);
    }

    // This function returns a single closest point. There may be more than one point on the path at the same distance.
    // Use ComputeApproxParamPerUnitLength to determine a good paramThreshold. eg. Say you want a 15cm threshold,
    // use: paramThreshold = ComputeApproxParamPerUnitLength() * 0.15f.
    public float ComputeClosestParam(Vector3 pos, float paramThreshold)
    {
        var minDistSq = float.MaxValue;
        var closestParam = 0.0f;
        for (var startIndex = 0; startIndex < controlVerts.Length - 1; startIndex += 3)
        {

            var curve = new CubicBezierCurve(controlVerts[startIndex + 0], controlVerts[startIndex + 1], controlVerts[startIndex + 2], controlVerts[startIndex + 3]);
            var curveClosestParam = curve.GetClosestParam(pos, paramThreshold);

            var curvePos = curve.GetPoint(curveClosestParam);
            var distSq = (curvePos - pos).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                var startParam = ((float)startIndex) / 3.0f;
                closestParam = startParam + curveClosestParam;
            }
        }

        return closestParam;
    }

    // Same as above but returns a t value E [0, 1]. You'll need to use a paramThreshold like
    // ComputeApproxParamPerUnitLength() * 0.15f if you want a 15cm tolerance.
    public float ComputeClosestNormParam(Vector3 pos, float paramThreshold)
    {
        return ComputeClosestParam(pos, paramThreshold * (float)numCurveSegments);
    }
}
