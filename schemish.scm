;; Copyright (c) 2022 Eliza Velasquez, Microsoft
;;
;; Permission is hereby granted, free of charge, to any person obtaining a copy
;; of this software and associated documentation files (the "Software"), to deal
;; in the Software without restriction, including without limitation the rights
;; to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
;; copies of the Software, and to permit persons to whom the Software is
;; furnished to do so, subject to the following conditions:
;;
;; The above copyright notice and this permission notice shall be included in
;; all copies or substantial portions of the Software.
;;
;; THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
;; IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
;; FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
;; AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
;; LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
;; OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
;; SOFTWARE.

;; This file defines a subset of the standard Scheme procedures and macros. It
;; is intended to be embedded in the client software and loaded into the
;; interpreter before other client-specific code.

(define (caar x) (car (car x)))
(define (cadr x) (car (cdr x)))
(define (cdar x) (cdr (car x)))
(define (cddr x) (cdr (cdr x)))
(define (caaar x) (car (car (car x))))
(define (caadr x) (car (car (cdr x))))
(define (cadar x) (car (cdr (car x))))
(define (caddr x) (car (cdr (cdr x))))
(define (cdaar x) (cdr (car (car x))))
(define (cdadr x) (cdr (car (cdr x))))
(define (cddar x) (cdr (cdr (car x))))
(define (cdddr x) (cdr (cdr (cdr x))))
(define (caaaar x) (car (car (car (car x)))))
(define (caaadr x) (car (car (car (cdr x)))))
(define (caadar x) (car (car (cdr (car x)))))
(define (caaddr x) (car (car (cdr (cdr x)))))
(define (cadaar x) (car (cdr (car (car x)))))
(define (cadadr x) (car (cdr (car (cdr x)))))
(define (caddar x) (car (cdr (cdr (car x)))))
(define (cadddr x) (car (cdr (cdr (cdr x)))))
(define (cdaaar x) (cdr (car (car (car x)))))
(define (cdaadr x) (cdr (car (car (cdr x)))))
(define (cdadar x) (cdr (car (cdr (car x)))))
(define (cdaddr x) (cdr (car (cdr (cdr x)))))
(define (cddaar x) (cdr (cdr (car (car x)))))
(define (cddadr x) (cdr (cdr (car (cdr x)))))
(define (cdddar x) (cdr (cdr (cdr (car x)))))
(define (cddddr x) (cdr (cdr (cdr (cdr x)))))

(define-macro let
              (lambda args
                (define specs (car args)) ; ( (var1 val1), ... )
                (define bodies (cdr args)) ; (expr1 ...)
                (if (null? specs)
                  `((lambda () ,@bodies))
                  (begin
                    (define spec1 (car specs)) ; (var1 val1)
                    (define spec_rest (cdr specs)) ; ((var2 val2) ...)
                    (define inner `((lambda ,(list (car spec1)) ,@bodies) ,(car (cdr spec1))))
                    `(let ,spec_rest ,inner)))))

(define-macro cond
              (lambda args
                (if (= 0 (length args)) ''()
                  (begin
                    (define first (car args))
                    (define rest (cdr args))
                    (define test1 (if (equal? (car first) 'else) '#t (car first)))
                    (define expr1 (car (cdr first)))
                    `(if ,test1 ,expr1 (cond ,@rest))))))

(define and
  (lambda args
    (if (null? args) #t
        (if (car args)
            (apply and (cdr args))
            #f))))

(define or
  (lambda args
    (if (null? args) #f
        (if (car args)
            #t
            (apply or (cdr args))))))

(define-macro when
  (lambda args
    (let ((test (car args))
          (bodies (cdr args)))
      `(if ,test (begin ,@bodies)))))

(define-macro unless
  (lambda args
    (let ((test (car args))
          (bodies (cdr args)))
      `(if (not ,test) (begin ,@bodies)))))

(define-macro while
  (lambda args
    (let ((test (car args))
          (body (cdr args)))
      `(begin
         (define (_loop)
           (if ,test (begin ,@body (_loop))))
         (_loop)))))

(define-macro (push item place)
  `(set! ,place (cons ,item ,place)))

(define (member x lst)
  (cond
   ((null? lst) #f)
   ((equal? x (car lst)) lst)
   (#t (member x (cdr lst)))))

(define-macro match-let
  (lambda args
    (let ((identifiers (caar args))
          (values (cadar args))
          (bodies (cdr args)))
      (if (or (null? identifiers) (null? values))
          `((lambda () ,@bodies))
          (let ((identifier1 (car identifiers))
                (identifiers-rest (cdr identifiers))
                (value1 (car values))
                (values-rest (cdr values)))
            (define inner `((lambda () (define ,identifier1 ,value1) ,@bodies)))
            `(match-let (,identifiers-rest ,values-rest) ,inner))))))
