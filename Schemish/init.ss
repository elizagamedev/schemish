(define (caar x)   (car (car x)))
(define (cadr x)   (car (cdr x)))
(define (cdar x)   (cdr (car x)))
(define (cddr x)   (cdr (cdr x)))
(define (caaar x)  (car (car (car x))))
(define (caadr x)  (car (car (cdr x))))
(define (cadar x)  (car (cdr (car x))))
(define (caddr x)  (car (cdr (cdr x))))
(define (cdaar x)  (cdr (car (car x))))
(define (cdadr x)  (cdr (car (cdr x))))
(define (cddar x)  (cdr (cdr (car x))))
(define (cdddr x)  (cdr (cdr (cdr x))))
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

(define-macro while
  (lambda args
    (let ((test (car args))
          (body (cdr args)))
      `(begin
         (define (_loop)
           (if ,cond (begin ,@body (_loop))))
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
