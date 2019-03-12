﻿/*
 Highcharts JS v6.0.5 (2018-01-31)
 Old IE (v6, v7, v8) module for Highcharts v6+.

 (c) 2010-2017 Highsoft AS
 Author: Torstein Honsi

 License: www.highcharts.com/license
*/
(function (l) { "object" === typeof module && module.exports ? module.exports = l : l(Highcharts) })(function (l) {
    (function (d) {
        var w, g; g = d.Chart; var A = d.createElement, l = d.css, E = d.defined, n = d.deg2rad, F = d.discardElement, h = d.doc, I = d.each, G = d.erase, B = d.extend; w = d.extendClass; var M = d.isArray, K = d.isNumber, C = d.isObject, N = d.merge, L = d.noop, x = d.pick, t = d.pInt, D = d.svg, y = d.SVGElement, u = d.SVGRenderer, v = d.win, O = d.wrap; d.getOptions().global.VMLRadialGradientURL = "http://code.highcharts.com/6.0.5/gfx/vml-radial-gradient.png"; h &&
            !h.defaultView && (d.getStyle = function (a, b) { var c = { width: "clientWidth", height: "clientHeight" }[b]; if (a.style[b]) return d.pInt(a.style[b]); "opacity" === b && (b = "filter"); if (c) return a.style.zoom = 1, Math.max(a[c] - 2 * d.getStyle(a, "padding"), 0); a = a.currentStyle[b.replace(/\-(\w)/g, function (a, b) { return b.toUpperCase() })]; "filter" === b && (a = a.replace(/alpha\(opacity=([0-9]+)\)/, function (a, b) { return b / 100 })); return "" === a ? 1 : d.pInt(a) }); Array.prototype.forEach || (d.forEachPolyfill = function (a, b) {
                for (var c = 0, e = this.length; c <
                    e; c++)if (!1 === a.call(b, this[c], c, this)) return c
            }); Array.prototype.indexOf || (d.indexOfPolyfill = function (a) { var b, c = 0; if (a) for (b = a.length; c < b; c++)if (a[c] === this) return c; return -1 }); Array.prototype.filter || (d.filterPolyfill = function (a) { for (var b = [], c = 0, e = this.length; c < e; c++)a(this[c], c) && b.push(this[c]); return b }); Object.prototype.keys || (d.keysPolyfill = function (a) { var b = [], c = Object.prototype.hasOwnProperty, e; for (e in a) c.call(a, e) && b.push(e); return b }); Array.prototype.reduce || (d.reducePolyfill = function (a,
                b) { b = b || {}; for (var c = this.length, e = 0; e < c; ++e)b = a.call(this, b, this[e], e, this); return b }); D || (O(d.SVGRenderer.prototype, "text", function (a) { return a.apply(this, Array.prototype.slice.call(arguments, 1)).css({ position: "absolute" }) }), d.Pointer.prototype.normalize = function (a, b) { a = a || v.event; a.target || (a.target = a.srcElement); b || (this.chartPosition = b = d.offset(this.chart.container)); return d.extend(a, { chartX: Math.round(Math.max(a.x, a.clientX - b.left)), chartY: Math.round(a.y) }) }, g.prototype.ieSanitizeSVG = function (a) {
                    return a =
                        a.replace(/<IMG /g, "\x3cimage ").replace(/<(\/?)TITLE>/g, "\x3c$1title\x3e").replace(/height=([^" ]+)/g, 'height\x3d"$1"').replace(/width=([^" ]+)/g, 'width\x3d"$1"').replace(/hc-svg-href="([^"]+)">/g, 'xlink:href\x3d"$1"/\x3e').replace(/ id=([^" >]+)/g, ' id\x3d"$1"').replace(/class=([^" >]+)/g, 'class\x3d"$1"').replace(/ transform /g, " ").replace(/:(path|rect)/g, "$1").replace(/style="([^"]+)"/g, function (a) { return a.toLowerCase() })
                }, g.prototype.isReadyToRender = function () {
                    var a = this; return D || v != v.top ||
                        "complete" === h.readyState ? !0 : (h.attachEvent("onreadystatechange", function () { h.detachEvent("onreadystatechange", a.firstRender); "complete" === h.readyState && a.firstRender() }), !1)
                }, h.createElementNS || (h.createElementNS = function (a, b) { return h.createElement(b) }), d.addEventListenerPolyfill = function (a, b) { function c(a) { a.target = a.srcElement || v; b.call(e, a) } var e = this; e.attachEvent && (e.hcEventsIE || (e.hcEventsIE = {}), b.hcKey || (b.hcKey = d.uniqueKey()), e.hcEventsIE[b.hcKey] = c, e.attachEvent("on" + a, c)) }, d.removeEventListenerPolyfill =
                    function (a, b) { this.detachEvent && (b = this.hcEventsIE[b.hcKey], this.detachEvent("on" + a, b)) }, g = {
                        docMode8: h && 8 === h.documentMode, init: function (a, b) { var c = ["\x3c", b, ' filled\x3d"f" stroked\x3d"f"'], e = ["position: ", "absolute", ";"], d = "div" === b; ("shape" === b || d) && e.push("left:0;top:0;width:1px;height:1px;"); e.push("visibility: ", d ? "hidden" : "visible"); c.push(' style\x3d"', e.join(""), '"/\x3e'); b && (c = d || "span" === b || "img" === b ? c.join("") : a.prepVML(c), this.element = A(c)); this.renderer = a }, add: function (a) {
                            var b = this.renderer,
                            c = this.element, e = b.box, d = a && a.inverted, e = a ? a.element || a : e; a && (this.parentGroup = a); d && b.invertChild(c, e); e.appendChild(c); this.added = !0; this.alignOnAdd && !this.deferUpdateTransform && this.updateTransform(); if (this.onAdd) this.onAdd(); this.className && this.attr("class", this.className); return this
                        }, updateTransform: y.prototype.htmlUpdateTransform, setSpanRotation: function () {
                            var a = this.rotation, b = Math.cos(a * n), c = Math.sin(a * n); l(this.element, {
                                filter: a ? ["progid:DXImageTransform.Microsoft.Matrix(M11\x3d", b, ", M12\x3d",
                                    -c, ", M21\x3d", c, ", M22\x3d", b, ", sizingMethod\x3d'auto expand')"].join("") : "none"
                            })
                        }, getSpanCorrection: function (a, b, c, e, d) { var f = e ? Math.cos(e * n) : 1, p = e ? Math.sin(e * n) : 0, q = x(this.elemHeight, this.element.offsetHeight), h; this.xCorr = 0 > f && -a; this.yCorr = 0 > p && -q; h = 0 > f * p; this.xCorr += p * b * (h ? 1 - c : c); this.yCorr -= f * b * (e ? h ? c : 1 - c : 1); d && "left" !== d && (this.xCorr -= a * c * (0 > f ? -1 : 1), e && (this.yCorr -= q * c * (0 > p ? -1 : 1)), l(this.element, { textAlign: d })) }, pathToVML: function (a) {
                            for (var b = a.length, c = []; b--;)K(a[b]) ? c[b] = Math.round(10 *
                                a[b]) - 5 : "Z" === a[b] ? c[b] = "x" : (c[b] = a[b], !a.isArc || "wa" !== a[b] && "at" !== a[b] || (c[b + 5] === c[b + 7] && (c[b + 7] += a[b + 7] > a[b + 5] ? 1 : -1), c[b + 6] === c[b + 8] && (c[b + 8] += a[b + 8] > a[b + 6] ? 1 : -1))); return c.join(" ") || "x"
                        }, clip: function (a) { var b = this, c; a ? (c = a.members, G(c, b), c.push(b), b.destroyClip = function () { G(c, b) }, a = a.getCSS(b)) : (b.destroyClip && b.destroyClip(), a = { clip: b.docMode8 ? "inherit" : "rect(auto)" }); return b.css(a) }, css: y.prototype.htmlCss, safeRemoveChild: function (a) { a.parentNode && F(a) }, destroy: function () {
                        this.destroyClip &&
                            this.destroyClip(); return y.prototype.destroy.apply(this)
                        }, on: function (a, b) { this.element["on" + a] = function () { var a = v.event; a.target = a.srcElement; b(a) }; return this }, cutOffPath: function (a, b) { var c; a = a.split(/[ ,]/); c = a.length; if (9 === c || 11 === c) a[c - 4] = a[c - 2] = t(a[c - 2]) - 10 * b; return a.join(" ") }, shadow: function (a, b, c) {
                            var e = [], d, f = this.element, h = this.renderer, q, g = f.style, k, r = f.path, J, m, l, n; r && "string" !== typeof r.value && (r = "x"); m = r; if (a) {
                                l = x(a.width, 3); n = (a.opacity || .15) / l; for (d = 1; 3 >= d; d++)J = 2 * l + 1 - 2 * d, c &&
                                    (m = this.cutOffPath(r.value, J + .5)), k = ['\x3cshape isShadow\x3d"true" strokeweight\x3d"', J, '" filled\x3d"false" path\x3d"', m, '" coordsize\x3d"10 10" style\x3d"', f.style.cssText, '" /\x3e'], q = A(h.prepVML(k), null, { left: t(g.left) + x(a.offsetX, 1), top: t(g.top) + x(a.offsetY, 1) }), c && (q.cutOff = J + 1), k = ['\x3cstroke color\x3d"', a.color || "#000000", '" opacity\x3d"', n * d, '"/\x3e'], A(h.prepVML(k), null, null, q), b ? b.element.appendChild(q) : f.parentNode.insertBefore(q, f), e.push(q); this.shadows = e
                            } return this
                        }, updateShadows: L,
                        setAttr: function (a, b) { this.docMode8 ? this.element[a] = b : this.element.setAttribute(a, b) }, classSetter: function (a) { (this.added ? this.element : this).className = a }, dashstyleSetter: function (a, b, c) { (c.getElementsByTagName("stroke")[0] || A(this.renderer.prepVML(["\x3cstroke/\x3e"]), null, null, c))[b] = a || "solid"; this[b] = a }, dSetter: function (a, b, c) {
                            var e = this.shadows; a = a || []; this.d = a.join && a.join(" "); c.path = a = this.pathToVML(a); if (e) for (c = e.length; c--;)e[c].path = e[c].cutOff ? this.cutOffPath(a, e[c].cutOff) : a; this.setAttr(b,
                                a)
                        }, fillSetter: function (a, b, c) { var e = c.nodeName; "SPAN" === e ? c.style.color = a : "IMG" !== e && (c.filled = "none" !== a, this.setAttr("fillcolor", this.renderer.color(a, c, b, this))) }, "fill-opacitySetter": function (a, b, c) { A(this.renderer.prepVML(["\x3c", b.split("-")[0], ' opacity\x3d"', a, '"/\x3e']), null, null, c) }, opacitySetter: L, rotationSetter: function (a, b, c) { c = c.style; this[b] = c[b] = a; c.left = -Math.round(Math.sin(a * n) + 1) + "px"; c.top = Math.round(Math.cos(a * n)) + "px" }, strokeSetter: function (a, b, c) {
                            this.setAttr("strokecolor",
                                this.renderer.color(a, c, b, this))
                        }, "stroke-widthSetter": function (a, b, c) { c.stroked = !!a; this[b] = a; K(a) && (a += "px"); this.setAttr("strokeweight", a) }, titleSetter: function (a, b) { this.setAttr(b, a) }, visibilitySetter: function (a, b, c) { "inherit" === a && (a = "visible"); this.shadows && I(this.shadows, function (c) { c.style[b] = a }); "DIV" === c.nodeName && (a = "hidden" === a ? "-999em" : 0, this.docMode8 || (c.style[b] = a ? "visible" : "hidden"), b = "top"); c.style[b] = a }, xSetter: function (a, b, c) {
                        this[b] = a; "x" === b ? b = "left" : "y" === b && (b = "top"); this.updateClipping ?
                            (this[b] = a, this.updateClipping()) : c.style[b] = a
                        }, zIndexSetter: function (a, b, c) { c.style[b] = a }
                    }, g["stroke-opacitySetter"] = g["fill-opacitySetter"], d.VMLElement = g = w(y, g), g.prototype.ySetter = g.prototype.widthSetter = g.prototype.heightSetter = g.prototype.xSetter, g = {
                        Element: g, isIE8: -1 < v.navigator.userAgent.indexOf("MSIE 8.0"), init: function (a, b, c) {
                            var e, d; this.alignedObjects = []; e = this.createElement("div").css({ position: "relative" }); d = e.element; a.appendChild(e.element); this.isVML = !0; this.box = d; this.boxWrapper =
                                e; this.gradients = {}; this.cache = {}; this.cacheKeys = []; this.imgCount = 0; this.setSize(b, c, !1); if (!h.namespaces.hcv) { h.namespaces.add("hcv", "urn:schemas-microsoft-com:vml"); try { h.createStyleSheet().cssText = "hcv\\:fill, hcv\\:path, hcv\\:shape, hcv\\:stroke{ behavior:url(#default#VML); display: inline-block; } " } catch (f) { h.styleSheets[0].cssText += "hcv\\:fill, hcv\\:path, hcv\\:shape, hcv\\:stroke{ behavior:url(#default#VML); display: inline-block; } " } }
                        }, isHidden: function () { return !this.box.offsetWidth },
                        clipRect: function (a, b, c, e) {
                            var d = this.createElement(), f = C(a); return B(d, {
                                members: [], count: 0, left: (f ? a.x : a) + 1, top: (f ? a.y : b) + 1, width: (f ? a.width : c) - 1, height: (f ? a.height : e) - 1, getCSS: function (a) { var b = a.element, c = b.nodeName, e = a.inverted, d = this.top - ("shape" === c ? b.offsetTop : 0), f = this.left, b = f + this.width, p = d + this.height, d = { clip: "rect(" + Math.round(e ? f : d) + "px," + Math.round(e ? p : b) + "px," + Math.round(e ? b : p) + "px," + Math.round(e ? d : f) + "px)" }; !e && a.docMode8 && "DIV" === c && B(d, { width: b + "px", height: p + "px" }); return d }, updateClipping: function () {
                                    I(d.members,
                                        function (a) { a.element && a.css(d.getCSS(a)) })
                                }
                            })
                        }, color: function (a, b, c, e) {
                            var p = this, f, h = /^rgba/, q, g, k = "none"; a && a.linearGradient ? g = "gradient" : a && a.radialGradient && (g = "pattern"); if (g) {
                                var r, l, m = a.linearGradient || a.radialGradient, n, t, u, v, w, x = ""; a = a.stops; var y, B = [], C = function () { q = ['\x3cfill colors\x3d"' + B.join(",") + '" opacity\x3d"', u, '" o:opacity2\x3d"', t, '" type\x3d"', g, '" ', x, 'focus\x3d"100%" method\x3d"any" /\x3e']; A(p.prepVML(q), null, null, b) }; n = a[0]; y = a[a.length - 1]; 0 < n[0] && a.unshift([0, n[1]]); 1 >
                                    y[0] && a.push([1, y[1]]); I(a, function (a, b) { h.test(a[1]) ? (f = d.color(a[1]), r = f.get("rgb"), l = f.get("a")) : (r = a[1], l = 1); B.push(100 * a[0] + "% " + r); b ? (u = l, v = r) : (t = l, w = r) }); if ("fill" === c) if ("gradient" === g) c = m.x1 || m[0] || 0, a = m.y1 || m[1] || 0, n = m.x2 || m[2] || 0, m = m.y2 || m[3] || 0, x = 'angle\x3d"' + (90 - 180 * Math.atan((m - a) / (n - c)) / Math.PI) + '"', C(); else {
                                        var k = m.r, D = 2 * k, E = 2 * k, F = m.cx, G = m.cy, H = b.radialReference, z, k = function () {
                                            H && (z = e.getBBox(), F += (H[0] - z.x) / z.width - .5, G += (H[1] - z.y) / z.height - .5, D *= H[2] / z.width, E *= H[2] / z.height); x =
                                                'src\x3d"' + d.getOptions().global.VMLRadialGradientURL + '" size\x3d"' + D + "," + E + '" origin\x3d"0.5,0.5" position\x3d"' + F + "," + G + '" color2\x3d"' + w + '" '; C()
                                        }; e.added ? k() : e.onAdd = k; k = v
                                    } else k = r
                            } else h.test(a) && "IMG" !== b.tagName ? (f = d.color(a), e[c + "-opacitySetter"](f.get("a"), c, b), k = f.get("rgb")) : (k = b.getElementsByTagName(c), k.length && (k[0].opacity = 1, k[0].type = "solid"), k = a); return k
                        }, prepVML: function (a) {
                            var b = this.isIE8; a = a.join(""); b ? (a = a.replace("/\x3e", ' xmlns\x3d"urn:schemas-microsoft-com:vml" /\x3e'), a =
                                -1 === a.indexOf('style\x3d"') ? a.replace("/\x3e", ' style\x3d"display:inline-block;behavior:url(#default#VML);" /\x3e') : a.replace('style\x3d"', 'style\x3d"display:inline-block;behavior:url(#default#VML);')) : a = a.replace("\x3c", "\x3chcv:"); return a
                        }, text: u.prototype.html, path: function (a) { var b = { coordsize: "10 10" }; M(a) ? b.d = a : C(a) && B(b, a); return this.createElement("shape").attr(b) }, circle: function (a, b, c) { var e = this.symbol("circle"); C(a) && (c = a.r, b = a.y, a = a.x); e.isCircle = !0; e.r = c; return e.attr({ x: a, y: b }) }, g: function (a) {
                            var b;
                            a && (b = { className: "highcharts-" + a, "class": "highcharts-" + a }); return this.createElement("div").attr(b)
                        }, image: function (a, b, c, e, d) { var f = this.createElement("img").attr({ src: a }); 1 < arguments.length && f.attr({ x: b, y: c, width: e, height: d }); return f }, createElement: function (a) { return "rect" === a ? this.symbol(a) : u.prototype.createElement.call(this, a) }, invertChild: function (a, b) {
                            var c = this; b = b.style; var e = "IMG" === a.tagName && a.style; l(a, { flip: "x", left: t(b.width) - (e ? t(e.top) : 1), top: t(b.height) - (e ? t(e.left) : 1), rotation: -90 });
                            I(a.childNodes, function (b) { c.invertChild(b, a) })
                        }, symbols: {
                            arc: function (a, b, c, e, d) { var f = d.start, h = d.end, g = d.r || c || e; c = d.innerR; e = Math.cos(f); var p = Math.sin(f), k = Math.cos(h), l = Math.sin(h); if (0 === h - f) return ["x"]; f = ["wa", a - g, b - g, a + g, b + g, a + g * e, b + g * p, a + g * k, b + g * l]; d.open && !c && f.push("e", "M", a, b); f.push("at", a - c, b - c, a + c, b + c, a + c * k, b + c * l, a + c * e, b + c * p, "x", "e"); f.isArc = !0; return f }, circle: function (a, b, c, e, d) { d && E(d.r) && (c = e = 2 * d.r); d && d.isCircle && (a -= c / 2, b -= e / 2); return ["wa", a, b, a + c, b + e, a + c, b + e / 2, a + c, b + e / 2, "e"] },
                            rect: function (a, b, c, d, g) { return u.prototype.symbols[E(g) && g.r ? "callout" : "square"].call(0, a, b, c, d, g) }
                        }
                    }, d.VMLRenderer = w = function () { this.init.apply(this, arguments) }, w.prototype = N(u.prototype, g), d.Renderer = w); u.prototype.getSpanWidth = function (a, b) { var c = a.getBBox(!0).width; !D && this.forExport && (c = this.measureSpanWidth(b.firstChild.data, a.styles)); return c }; u.prototype.measureSpanWidth = function (a, b) {
                        var c = h.createElement("span"); a = h.createTextNode(a); c.appendChild(a); l(c, b); this.box.appendChild(c); b = c.offsetWidth;
                        F(c); return b
                    }
    })(l)
});
